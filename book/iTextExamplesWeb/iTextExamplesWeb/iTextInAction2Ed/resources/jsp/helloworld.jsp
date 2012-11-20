<%@
page import="java.io.*, com.itextpdf.text.*, com.itextpdf.text.pdf.*"
%><%
response.setContentType( "application/pdf" );

// step 1:
Document document = new Document();
// step 2:
ByteArrayOutputStream buffer = new ByteArrayOutputStream();
PdfWriter.getInstance( document, buffer );
// step 3:
document.open();
// step 4:
document.add(new Paragraph("Hello World"));
// step 5:
document.close();
// we output the writer as bytes to the response output
DataOutput output = new DataOutputStream( response.getOutputStream() );
byte[] bytes = buffer.toByteArray();
response.setContentLength(bytes.length);
for( int i = 0; i < bytes.length; i++ ) { output.writeByte( bytes[i] ); }
%>