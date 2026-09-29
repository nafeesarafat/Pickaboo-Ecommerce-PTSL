using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class ReportService : IReportService
{
    private const int LowStockThreshold = AdminInventoryService.DefaultLowStockThreshold;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ReportService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<SalesReportDto> GetSalesAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        var orders = SalesOrdersInRange(query);
        return new SalesReportDto
        {
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            TotalSales = await orders.SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0m,
            TotalOrders = await orders.CountAsync(cancellationToken)
        };
    }

    public async Task<RevenueReportDto> GetRevenueAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        return new RevenueReportDto
        {
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            TotalRevenue = await OrdersInRange(query)
                .Where(order => order.OrderStatus == "Delivered")
                .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0m
        };
    }

    public async Task<IReadOnlyList<OrderReportDto>> GetOrdersAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        return await OrdersInRange(query)
            .GroupBy(order => order.OrderStatus)
            .Select(group => new OrderReportDto
            {
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                OrderStatus = group.Key,
                TotalOrders = group.Count()
            })
            .OrderBy(report => report.OrderStatus)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductReportDto>> GetProductsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        return await _context.OrderItems.AsNoTracking()
            .Where(item => item.Order.OrderDate >= query.FromDate
                && item.Order.OrderDate <= query.ToDate
                && item.Order.OrderStatus != "Cancelled")
            .GroupBy(item => item.ProductId)
            .Select(group => new ProductReportDto
            {
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                ProductId = group.Key,
                UnitsSold = group.Sum(item => item.Quantity)
            })
            .OrderByDescending(report => report.UnitsSold)
            .ThenBy(report => report.ProductId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<CustomerReportDto> GetCustomersAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        DateTime endExclusive = query.ToDate;
        return new CustomerReportDto
        {
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            NewCustomers = await _context.UserInformation.AsNoTracking()
                .CountAsync(user => user.Role == "Customer" && user.RegistrationDate >= query.FromDate && user.RegistrationDate <= endExclusive, cancellationToken),
            ActiveCustomers = await OrdersInRange(query)
                .Where(order => order.User.Role == "Customer" && order.OrderStatus != "Cancelled")
                .Select(order => order.UserId).Distinct().CountAsync(cancellationToken)
        };
    }

    public async Task<InventoryReportDto> GetInventoryAsync(CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(products => new InventoryReportDto
            {
                TotalStock = products.Sum(product => product.StockQuantity),
                LowStockCount = products.Count(product => product.StockQuantity > 0 && product.StockQuantity <= LowStockThreshold),
                OutOfStockCount = products.Count(product => product.StockQuantity == 0)
            })
            .FirstOrDefaultAsync(cancellationToken) ?? new InventoryReportDto();
    }

    public async Task<PaymentReportDto> GetPaymentsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        // Payment transactions are not persisted separately; delivered orders are the available
        // completed-payment source in the current data model.
        return new PaymentReportDto
        {
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            TotalPayments = await OrdersInRange(query)
                .Where(order => order.OrderStatus == "Delivered")
                .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0m
        };
    }

    public async Task<DiscountReportDto> GetDiscountsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        // Coupon currently stores lifetime usage and expiry, not individual redemption dates.
        // The report therefore scopes existing usage totals by the only available coupon date.
        var coupons = _context.Coupons.AsNoTracking()
            .Where(coupon => coupon.ExpiryDate >= query.FromDate && coupon.ExpiryDate <= query.ToDate && coupon.UsageCount > 0);
        return new DiscountReportDto
        {
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            TotalDiscountAmount = await coupons.SumAsync(coupon => (decimal?)(coupon.DiscountAmount * coupon.UsageCount), cancellationToken) ?? 0m,
            CouponsUsed = await coupons.SumAsync(coupon => (int?)coupon.UsageCount, cancellationToken) ?? 0
        };
    }

    public Task<IReadOnlyList<SalesByCategoryDto>> GetSalesByCategoryAsync(CancellationToken cancellationToken) =>
        GetSalesByCategoryAsync(null, cancellationToken);

    private async Task<IReadOnlyList<SalesByCategoryDto>> GetSalesByCategoryAsync(
        ReportDateRangeQueryDto? query,
        CancellationToken cancellationToken)
    {
        var items = SalesItems(query);
        return await items.GroupBy(item => new { item.Product.CategoryId, item.Product.Category.CategoryName })
            .Select(group => new SalesByCategoryDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                TotalSales = group.Sum(item => item.TotalAmount)
            })
            .OrderByDescending(report => report.TotalSales)
            .ToArrayAsync(cancellationToken);
    }

    public Task<IReadOnlyList<SalesByBrandDto>> GetSalesByBrandAsync(CancellationToken cancellationToken) =>
        GetSalesByBrandAsync(null, cancellationToken);

    private async Task<IReadOnlyList<SalesByBrandDto>> GetSalesByBrandAsync(
        ReportDateRangeQueryDto? query,
        CancellationToken cancellationToken)
    {
        var items = SalesItems(query);
        return await items.GroupBy(item => new { item.Product.BrandId, item.Product.Brand.BrandName })
            .Select(group => new SalesByBrandDto
            {
                BrandId = group.Key.BrandId,
                BrandName = group.Key.BrandName,
                TotalSales = group.Sum(item => item.TotalAmount)
            })
            .OrderByDescending(report => report.TotalSales)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SalesByDateDto>> GetSalesByDateAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken)
    {
        ValidateRange(query);
        return await SalesOrdersInRange(query)
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new SalesByDateDto
            {
                Date = group.Key,
                TotalSales = group.Sum(order => order.TotalAmount)
            })
            .OrderBy(report => report.Date)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ExportReportDto> ExportAsync(ExportReportRequestDto request, CancellationToken cancellationToken)
    {
        ValidateRange(request);
        string format = request.Format.Trim().ToLowerInvariant();
        string extension = format switch
        {
            "csv" => ".csv",
            "excel" or "xlsx" => ".xlsx",
            "pdf" => ".pdf",
            _ => throw new BadRequestException("Format must be PDF, CSV, or Excel.")
        };

        var rows = await GetExportRowsAsync(request, cancellationToken);
        byte[] content = format switch
        {
            "csv" => CreateCsv(rows),
            "excel" or "xlsx" => CreateExcel(rows),
            "pdf" => CreatePdf(rows, request.ReportType),
            _ => throw new InvalidOperationException("The validated export format is unsupported.")
        };

        string fileName = $"{Guid.NewGuid():N}{extension}";
        string directory = Path.Combine(_environment.ContentRootPath, "App_Data", "ReportExports");
        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), content, cancellationToken);

        return new ExportReportDto
        {
            ReportType = request.ReportType.Trim(),
            Format = extension[1..],
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            FileUrl = $"/api/admin/reports/download/{fileName}"
        };
    }

    public string GetExportPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Length > 40
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _)
            || !new[] { ".csv", ".xlsx", ".pdf" }.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
        {
            throw new NotFoundException("The report export was not found.");
        }

        string path = Path.Combine(_environment.ContentRootPath, "App_Data", "ReportExports", fileName);
        if (!File.Exists(path))
        {
            throw new NotFoundException("The report export was not found.");
        }

        return path;
    }

    private IQueryable<Order> OrdersInRange(ReportDateRangeQueryDto query) =>
        _context.Orders.AsNoTracking()
            .Where(order => order.OrderDate >= query.FromDate && order.OrderDate <= query.ToDate);

    private IQueryable<Order> SalesOrdersInRange(ReportDateRangeQueryDto query) =>
        OrdersInRange(query).Where(order => order.OrderStatus != "Cancelled");

    private IQueryable<OrderItem> SalesItems(ReportDateRangeQueryDto? query)
    {
        var items = _context.OrderItems.AsNoTracking()
            .Where(item => item.Order.OrderStatus != "Cancelled");
        return query is null
            ? items
            : items.Where(item => item.Order.OrderDate >= query.FromDate && item.Order.OrderDate <= query.ToDate);
    }

    private static void ValidateRange(ReportDateRangeQueryDto query)
    {
        if (query.FromDate == default || query.ToDate == default || query.FromDate > query.ToDate)
        {
            throw new BadRequestException("FromDate and ToDate are required, and FromDate must not be later than ToDate.");
        }
    }

    private async Task<IReadOnlyList<ReportExportRowDto>> GetExportRowsAsync(
        ExportReportRequestDto request,
        CancellationToken cancellationToken)
    {
        string report = request.ReportType.Trim().ToLowerInvariant();
        return report switch
        {
            "sales" => new[] { await GetSalesAsync(request, cancellationToken) }.Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = "Total", Count = item.TotalOrders, Amount = item.TotalSales }).ToArray(),
            "revenue" => new[] { await GetRevenueAsync(request, cancellationToken) }.Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = "Revenue", Amount = item.TotalRevenue }).ToArray(),
            "orders" => (await GetOrdersAsync(request, cancellationToken)).Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = item.OrderStatus, Count = item.TotalOrders }).ToArray(),
            "products" => (await GetProductsAsync(request, cancellationToken)).Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = item.ProductId.ToString(CultureInfo.InvariantCulture), Count = item.UnitsSold }).ToArray(),
            "customers" => (await GetCustomerExportRowsAsync(request, cancellationToken)),
            "payments" => new[] { await GetPaymentsAsync(request, cancellationToken) }.Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = "Payments (delivered orders)", Amount = item.TotalPayments }).ToArray(),
            "discounts" => new[] { await GetDiscountsAsync(request, cancellationToken) }.Select(item =>
                new ReportExportRowDto { Date = $"{item.FromDate:O} - {item.ToDate:O}", Group = "Coupon usage", Count = item.CouponsUsed, Amount = item.TotalDiscountAmount }).ToArray(),
            "inventory" => (await GetInventoryExportRowsAsync(cancellationToken)),
            "sales-by-category" => (await GetSalesByCategoryAsync(request, cancellationToken)).Select(item =>
                new ReportExportRowDto { Group = item.CategoryName, Amount = item.TotalSales }).ToArray(),
            "sales-by-brand" => (await GetSalesByBrandAsync(request, cancellationToken)).Select(item =>
                new ReportExportRowDto { Group = item.BrandName, Amount = item.TotalSales }).ToArray(),
            "sales-by-date" => (await GetSalesByDateAsync(request, cancellationToken)).Select(item =>
                new ReportExportRowDto { Date = item.Date.ToString("O", CultureInfo.InvariantCulture), Amount = item.TotalSales }).ToArray(),
            _ => throw new BadRequestException("ReportType is not supported.")
        };
    }

    private async Task<IReadOnlyList<ReportExportRowDto>> GetCustomerExportRowsAsync(
        ReportDateRangeQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await GetCustomersAsync(query, cancellationToken);
        string date = $"{result.FromDate:O} - {result.ToDate:O}";
        return new[]
        {
            new ReportExportRowDto { Date = date, Group = "New customers", Count = result.NewCustomers },
            new ReportExportRowDto { Date = date, Group = "Active customers", Count = result.ActiveCustomers }
        };
    }

    private async Task<IReadOnlyList<ReportExportRowDto>> GetInventoryExportRowsAsync(CancellationToken cancellationToken)
    {
        var result = await GetInventoryAsync(cancellationToken);
        return new[]
        {
            new ReportExportRowDto { Group = "Total stock units", Count = result.TotalStock },
            new ReportExportRowDto { Group = "Low-stock products", Count = result.LowStockCount },
            new ReportExportRowDto { Group = "Out-of-stock products", Count = result.OutOfStockCount }
        };
    }

    private static byte[] CreateCsv(IEnumerable<ReportExportRowDto> rows)
    {
        var builder = new StringBuilder("Date,Group,Count,Amount\r\n");
        foreach (var row in rows)
        {
            builder.Append(Csv(row.Date)).Append(',')
                .Append(Csv(row.Group)).Append(',')
                .Append(row.Count.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(row.Amount.ToString(CultureInfo.InvariantCulture)).Append("\r\n");
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(builder.ToString());
    }

    private static string Csv(string? value)
    {
        string safeValue = value ?? string.Empty;
        string trimmed = safeValue.TrimStart();
        if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0]))
        {
            safeValue = $"'{safeValue}";
        }

        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private static byte[] CreateExcel(IReadOnlyList<ReportExportRowDto> rows)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddZipText(archive, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            AddZipText(archive, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            AddZipText(archive, "xl/workbook.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Report\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            AddZipText(archive, "xl/_rels/workbook.xml.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");

            var sheet = archive.CreateEntry("xl/worksheets/sheet1.xml");
            using var stream = sheet.Open();
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), CloseOutput = false });
            writer.WriteStartDocument(true);
            writer.WriteStartElement("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            writer.WriteStartElement("sheetData");
            WriteExcelRow(writer, 1, "Date", "Group", "Count", "Amount");
            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                writer.WriteStartElement("row");
                writer.WriteAttributeString("r", (index + 2).ToString(CultureInfo.InvariantCulture));
                WriteExcelTextCell(writer, $"A{index + 2}", row.Date);
                WriteExcelTextCell(writer, $"B{index + 2}", row.Group);
                WriteExcelNumberCell(writer, $"C{index + 2}", row.Count.ToString(CultureInfo.InvariantCulture));
                WriteExcelNumberCell(writer, $"D{index + 2}", row.Amount.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return memory.ToArray();
    }

    private static void WriteExcelRow(XmlWriter writer, int rowNumber, string first, string second, string third, string fourth)
    {
        writer.WriteStartElement("row");
        writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));
        WriteExcelTextCell(writer, $"A{rowNumber}", first);
        WriteExcelTextCell(writer, $"B{rowNumber}", second);
        WriteExcelTextCell(writer, $"C{rowNumber}", third);
        WriteExcelTextCell(writer, $"D{rowNumber}", fourth);
        writer.WriteEndElement();
    }

    private static void WriteExcelTextCell(XmlWriter writer, string reference, string value)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("t", "inlineStr");
        writer.WriteStartElement("is");
        writer.WriteElementString("t", "http://schemas.openxmlformats.org/spreadsheetml/2006/main", value);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteExcelNumberCell(XmlWriter writer, string reference, string value)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteElementString("v", value);
        writer.WriteEndElement();
    }

    private static void AddZipText(ZipArchive archive, string name, string value)
    {
        var entry = archive.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(value);
    }

    private static byte[] CreatePdf(IReadOnlyList<ReportExportRowDto> rows, string title)
    {
        var pages = rows.Chunk(58).ToArray();
        if (pages.Length == 0)
        {
            pages = new[] { Array.Empty<ReportExportRowDto>() };
        }

        int fontObjectNumber = 3 + pages.Length * 2;
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, pages.Length).Select(index => $"{3 + index * 2} 0 R"))}] /Count {pages.Length} >>"
        };

        for (int index = 0; index < pages.Length; index++)
        {
            int pageObjectNumber = 3 + index * 2;
            int contentObjectNumber = pageObjectNumber + 1;
            var lines = new List<string> { title, "Date | Group | Count | Amount" };
            lines.AddRange(pages[index].Select(row =>
                $"{row.Date} | {row.Group} | {row.Count} | {row.Amount.ToString(CultureInfo.InvariantCulture)}"));

            var content = new StringBuilder("BT /F1 9 Tf 40 800 Td 12 TL\n");
            foreach (string line in lines)
            {
                string safe = new string(line.Select(character => character is >= ' ' and <= '~' ? character : '?').ToArray())
                    .Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
                content.Append('(').Append(safe).Append(") Tj T*\n");
            }

            content.Append("ET");
            byte[] streamBytes = Encoding.ASCII.GetBytes(content.ToString());
            objects.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 842] /Resources << /Font << /F1 {fontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
            objects.Add($"<< /Length {streamBytes.Length} >>\nstream\n{content}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        using var output = new MemoryStream();
        WriteAscii(output, "%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        foreach (string obj in objects)
        {
            offsets.Add(output.Position);
            WriteAscii(output, $"{offsets.Count - 1} 0 obj\n{obj}\nendobj\n");
        }

        long xrefOffset = output.Position;
        WriteAscii(output, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (long offset in offsets.Skip(1))
        {
            WriteAscii(output, $"{offset.ToString("0000000000", CultureInfo.InvariantCulture)} 00000 n \n");
        }

        WriteAscii(output, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return output.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
