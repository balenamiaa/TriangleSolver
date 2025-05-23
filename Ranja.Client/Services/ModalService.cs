namespace Ranja.Client.Services;

public class ModalService : IModalService
{
    private readonly Dictionary<string, ModalInfo> _activeModals = new();
    
    public event Action? OnModalStateChanged;

    public Task ShowModalAsync<T>(string modalId, T data, ModalOptions? options = null) where T : class
    {
        options ??= new ModalOptions();
        
        var modalInfo = new ModalInfo(
            modalId,
            data,
            options,
            DateTime.UtcNow
        );
        
        _activeModals[modalId] = modalInfo;
        OnModalStateChanged?.Invoke();
        
        return Task.CompletedTask;
    }

    public Task HideModalAsync(string modalId)
    {
        if (_activeModals.Remove(modalId))
        {
            OnModalStateChanged?.Invoke();
        }
        
        return Task.CompletedTask;
    }

    public Task HideAllModalsAsync()
    {
        if (_activeModals.Any())
        {
            _activeModals.Clear();
            OnModalStateChanged?.Invoke();
        }
        
        return Task.CompletedTask;
    }

    public bool IsModalVisible(string modalId)
    {
        return _activeModals.ContainsKey(modalId);
    }

    public T? GetModalData<T>(string modalId) where T : class
    {
        return _activeModals.TryGetValue(modalId, out var modal) ? modal.Data as T : null;
    }

    public IEnumerable<ModalInfo> GetActiveModals()
    {
        return _activeModals.Values.OrderBy(m => m.CreatedAt);
    }
} 