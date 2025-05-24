using Microsoft.AspNetCore.Components;

namespace TriangleSolver.Client.Services;

public interface IModalService
{
    event Action? OnModalStateChanged;
    
    Task ShowModalAsync<T>(string modalId, T data, ModalOptions? options = null) where T : class;
    Task HideModalAsync(string modalId);
    Task HideAllModalsAsync();
    Task UpdateModalPositionsAsync();
    void RegisterModalRefreshAction(string modalId, Func<Task> refreshAction);
    
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

public enum ModalPosition
{
    TopCenter,
    BottomCenter,
    LeftCenter,
    RightCenter,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public record ModalOptions(
    ElementReference? TargetElementRef = null,
    ModalPosition Position = ModalPosition.TopCenter,
    bool CloseOnBackdropClick = true,
    bool CloseOnEscape = true,
    string? CssClass = null,
    int ZIndex = 1000
); 