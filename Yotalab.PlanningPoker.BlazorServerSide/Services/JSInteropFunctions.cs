using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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

    /// <summary>
    /// Сгенерировать клик по элементу.
    /// </summary>
    /// <param name="element">Ссылка на элемент.</param>
    /// <returns>Задача на клик по элементу.</returns>
    public async Task ClickElement(ElementReference element)
    {
      await this.jsRuntime.InvokeVoidAsync("interopFunctions.clickElement", element);
    }

    public async Task<string> FrameInnerText(ElementReference frameElement)
    {
      return await this.jsRuntime.InvokeAsync<string>("interopFunctions.getFrameContentDocumentInnerText", frameElement);
    }
  }
}
