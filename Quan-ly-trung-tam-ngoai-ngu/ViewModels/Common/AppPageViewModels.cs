namespace Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

public class AppPageViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = [];
}

public class BreadcrumbItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}

public class SummaryCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-circle-square";
    public string AccentClass { get; set; } = "primary";
    public string Trend { get; set; } = string.Empty;
}

public class SelectOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Selected { get; set; }
}

public class FilterGroupViewModel
{
    public string Label { get; set; } = string.Empty;
    public string InputId { get; set; } = string.Empty;
    public string InputType { get; set; } = "select";
    public string Placeholder { get; set; } = string.Empty;
    public List<SelectOptionViewModel> Options { get; set; } = [];
}

public class TableColumnViewModel
{
    public string Header { get; set; } = string.Empty;
    public string? Width { get; set; }
}

public class TableCellViewModel
{
    public string Html { get; set; } = string.Empty;
}

public class TableRowActionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-arrow-right";
    public string CssClass { get; set; } = "btn btn-sm btn-outline-primary";
    public bool RequiresConfirm { get; set; }
    public string ConfirmMessage { get; set; } = "Bạn có chắc chắn muốn tiếp tục thao tác này?";
}

public class TableRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public List<TableCellViewModel> Cells { get; set; } = [];
    public List<TableRowActionViewModel> Actions { get; set; } = [];
}

public class TableViewModel
{
    public List<TableColumnViewModel> Columns { get; set; } = [];
    public List<TableRowViewModel> Rows { get; set; } = [];
    public string EmptyTitle { get; set; } = "Chưa có dữ liệu";
    public string EmptyDescription { get; set; } = "Dữ liệu sẽ hiển thị tại đây sau khi kết nối database.";
}

public class EmptyStateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-inbox";
    public string? ActionLabel { get; set; }
    public string? ActionUrl { get; set; }
}

public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 5;
}

public class QuickActionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-arrow-right";
    public string CssClass { get; set; } = "btn btn-primary";
}

public class DetailItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsBadge { get; set; }
    public string BadgeClass { get; set; } = "bg-primary-subtle text-primary";
}

public class DetailSectionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<DetailItemViewModel> Items { get; set; } = [];
}

public class TimelineItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AccentClass { get; set; } = "primary";
}

public class FormFieldViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string Placeholder { get; set; } = string.Empty;
    public string ColClass { get; set; } = "col-md-6";
    public bool Required { get; set; }
    public bool ReadOnly { get; set; }
    public string? Hint { get; set; }
    public List<SelectOptionViewModel> Options { get; set; } = [];
}

public class FormSectionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<FormFieldViewModel> Fields { get; set; } = [];
}

public class ManagementListPageViewModel : AppPageViewModel
{
    public string PrimaryActionText { get; set; } = string.Empty;
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string SearchPlaceholder { get; set; } = "Tìm kiếm...";
    public string? ToolbarNote { get; set; }
    public string? ExportLabel { get; set; }
    public List<SummaryCardViewModel> SummaryCards { get; set; } = [];
    public List<FilterGroupViewModel> Filters { get; set; } = [];
    public TableViewModel Table { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class ManagementFormPageViewModel : AppPageViewModel
{
    public string FormTitle { get; set; } = string.Empty;
    public string FormDescription { get; set; } = string.Empty;
    public string FormActionUrl { get; set; } = string.Empty;
    public string FormMethod { get; set; } = "post";
    public string SubmitLabel { get; set; } = "Lưu tạm";
    public string CancelLabel { get; set; } = "Quay lại";
    public string CancelUrl { get; set; } = string.Empty;
    public string? Notice { get; set; }
    public string? ErrorMessage { get; set; }
    public List<FormSectionViewModel> Sections { get; set; } = [];
}

public class ManagementDetailsPageViewModel : AppPageViewModel
{
    public List<SummaryCardViewModel> SummaryCards { get; set; } = [];
    public List<DetailSectionViewModel> Sections { get; set; } = [];
    public List<QuickActionViewModel> Actions { get; set; } = [];
    public List<TimelineItemViewModel> Timeline { get; set; } = [];
}
