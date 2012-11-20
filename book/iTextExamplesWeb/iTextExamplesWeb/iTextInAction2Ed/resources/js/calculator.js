var previous = 0;
var current = 0;
var operation = '';

function showCurrent() {
  this.getField('result').value = current;
}
function showMove(s) {
  this.getField('move').value = s;
}
function augment(digit) {
  current = current * 10 + digit;
  showCurrent();
}
function register(op) {
  previous = current;
  current = 0;
  operation = op;
  showCurrent();
}
function calculateResult() {
  if (operation == '+')
    current = previous + current;
  else if (operation == '-')
    current = previous - current;
  else if (operation == '*')
    current = previous * current;
  else if (operation == '/')
    current = previous / current;
  showCurrent();
}
function reset(all) {
  current = 0;
  if(all) previous = 0;
  showCurrent();
}
showCurrent();