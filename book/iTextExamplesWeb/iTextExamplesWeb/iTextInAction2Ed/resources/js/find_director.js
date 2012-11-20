function findDirector(name) {
  if (search.available) {
    search.query(name, "ActiveDoc");
  }
  else {
    app.alert("The Search plug-in isn't installed.");
  }
}