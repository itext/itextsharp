if(this.hostContainer) {
  var names = new Array();
  names[0] = this.getField("personal.name").value.toString();
  names[1] = this.getField("personal.loginname").value.toString();
  try{
    this.hostContainer.postMessage(names);
  }
  catch(e){
    app.alert(e.message);
  }
}