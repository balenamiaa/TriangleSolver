namespace Ranja.Client.Services;

public interface IModalService
{
    event Action? OnModalStateChanged;
    
    Task ShowModalAsync<T>(string modalId, T data, ModalOptions? options = null) where T : class;
    Task HideModalAsync(string modalId);
    Task HideAllModalsAsync();
    
    bool IsModalVisible(string modalId);
    T? GetModalData<T>(string modalId) where T : class;
    IEnumerable<ModalInfo> GetActiveModals();
}

public record ModalInfo(
    string Id,
    object Data,
    ModalOptions Options,
    DateTime CreatedAt
);

public record ModalOptions(
    double? X = null,
    double? Y = null,
    bool CloseOnBackdropClick = true,
    bool CloseOnEscape = true,
    string? CssClass = null,
    int ZIndex = 1000
); 