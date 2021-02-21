using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  /// <summary>
  /// Класс для взаимодействия с клиентскими javascript функциями.
  /// </summary>
  /// <remarks>Обертка над функциями из файла wwwroot/js/interop-functions.js.</remarks>
  public class JSInteropFunctions
  {
    private readonly IJSRuntime jsRuntime;

    public JSInteropFunctions(IJSRuntime jSRuntime)
    {
      this.jsRuntime = jSRuntime;
    }

    /// <summary>
    /// Сгенерировать клик по элементу.
    /// </summary>
    /// <param name="elementId">Идентификатор элемента.</param>
    /// <returns>Задача на клик по элементу.</returns>
    public async Task ClickElementById(string elementId)
    {
      if (string.IsNullOrWhiteSpace(elementId))
        return;

      await this.jsRuntime.InvokeVoidAsync("interopFunctions.clickElementById", elementId);
    }
  }
}
