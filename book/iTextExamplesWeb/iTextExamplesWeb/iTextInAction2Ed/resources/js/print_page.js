function printCurrentPage() {
  var pp = this.getPrintParams();
  pp.firstPage = this.pageNum;
  pp.lastPage = pp.firstPage;
  this.print(pp);
}