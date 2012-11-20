function showButtonState() {
  app.alert('Checkboxes:'
  + ' English: ' + this.getField('English').value
  + ' German: ' + this.getField('German').value
  + ' French: ' + this.getField('French').value
  + ' Spanish: ' + this.getField('Spanish').value
  + ' Dutch: ' + this.getField('Dutch').value
  + '; Radioboxes: ' + this.getField('language').value);
}