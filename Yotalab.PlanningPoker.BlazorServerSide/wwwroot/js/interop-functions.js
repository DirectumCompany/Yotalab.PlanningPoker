window.interopFunctions = {
  /**
   * Сгенерировать клик по элементу.
   * @param {any} elementId Идентификатор элемента.
   */
  clickElementById: function (elementId) {
    document.getElementById(elementId).click();
  },

  /**
   * Сгенерировать клик по элементу.
   * @param {any} element Ссылка на элемент.
   */
  clickElement: function (element) {
    if (element == null)
      return;

    element.click();
  },

  /**
   * Получить содержимое документа фрейма.
   * @param {any} frameElement Ссылка на фрейм.
   */
  getFrameContentDocumentInnerText: function (frameElement) {
    if (frameElement == null)
      return null;

    if (frameElement.contentDocument == null)
      return null;

    if (frameElement.contentDocument.body == null)
      return null;

    return frameElement.contentDocument.body.innerText;
  }
}
