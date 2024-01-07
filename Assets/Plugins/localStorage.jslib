mergeInto(LibraryManager.library, {
  LocalStorageGetItem: function (key) {
    var result = window.localStorage.getItem(UTF8ToString(key));
    var str = result !== null ? result : "";
    
    var bufferSize = lengthBytesUTF8(str) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(str, buffer, bufferSize);
    return buffer;
  },

  LocalStorageSetItem: function (key, value) {
    window.localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
  },

  LocalStorageRemoveItem: function(key) {
    window.localStorage.removeItem(UTF8ToString(key));
  },
});
