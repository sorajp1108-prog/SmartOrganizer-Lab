using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public interface IReadOnlyFileCatalog { Task<QueryResult> QueryAsync(string root,QueryOptions options,CancellationToken cancellationToken); }
