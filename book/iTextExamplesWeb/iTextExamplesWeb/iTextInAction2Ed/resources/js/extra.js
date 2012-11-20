function setReadOnly(readonly) {
  var partner = this.getField('partner');
  if(readonly) {
    partner.value = '';
  }
  partner.readonly = readonly;
}
function validate() {
  var married = this.getField('married');
  var partner = this.getField('partner');
  if (married.value == 'yes' && partner.value == '') {
    app.alert('please enter the name of your partner');
  }
  else {
    this.submitForm({
      cURL:"http://itextpdf.com:8080/book/request",
      cSubmitAs: "HTML"
    });
  }
}