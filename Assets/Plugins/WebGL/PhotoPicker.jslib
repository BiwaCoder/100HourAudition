mergeInto(LibraryManager.library, {
  PhotoPicker_Open: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);
    var input = document.getElementById('__photoPickerInput');
    if (!input) {
      input = document.createElement('input');
      input.type = 'file';
      input.accept = 'image/png,image/jpeg';
      input.style.display = 'none';
      input.id = '__photoPickerInput';
      document.body.appendChild(input);
    }
    // Reset so choosing the same file twice still fires onchange.
    input.value = '';
    input.onchange = function (e) {
      var file = e.target.files && e.target.files[0];
      if (!file) return;
      var reader = new FileReader();
      reader.onload = function (ev) {
        var dataUrl = ev.target.result;
        var base64 = dataUrl.substring(dataUrl.indexOf(',') + 1);
        SendMessage(objectName, methodName, base64);
      };
      reader.onerror = function () {
        SendMessage(objectName, methodName + 'Failed', 'read error');
      };
      reader.readAsDataURL(file);
    };
    input.click();
  }
});
