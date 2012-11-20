this.disclosed = true;
if (this.external && this.hostContainer) {
  function onMessageFunc( stringArray ) {
    var name = this.myDoc.getField("personal.name");
    var login = this.myDoc.getField("personal.loginname");
    try{
      name.value = stringArray[0];
      login.value = stringArray[1];
    }
    catch(e){
      onErrorFunc(e);
    }
  }
  function onErrorFunc( e ) {
      console.show();
      console.println(e.toString());
  }
  try {
    if(!this.hostContainer.messageHandler);
    this.hostContainer.messageHandler = new Object();
    this.hostContainer.messageHandler.myDoc = this;
    this.hostContainer.messageHandler.onMessage = onMessageFunc;
    this.hostContainer.messageHandler.onError = onErrorFunc;
    this.hostContainer.messageHandler.onDisclose = function(){return true;};
  }
   catch(e){
      onErrorFunc(e);
  }
}