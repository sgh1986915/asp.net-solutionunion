<%@ Page Language="C#" %><!DOCTYPE html>
<html>
<head>
   <script runat="server">
      
      protected int statusCode;
      protected string title = "";
      protected string message = "";

      protected void Page_Load(object sender, EventArgs e) {

         try {
            statusCode = this.Response.StatusCode;
            title = this.Response.StatusDescription;

            HttpException httpEx = Server.GetLastError() as HttpException;

            if (httpEx != null)
               message = httpEx.Message;

            if (String.IsNullOrEmpty(message)) {

               if (statusCode == 410)
                  message = "The URL you tried to use is either incorrect or no longer valid.";
               else if (statusCode == 500)
                  message = "An unexpected error ocurred, please try again later.";
            }
               
         } catch { 
            // Error page cannot fail
         }
      }
      
   </script>
   <title><%= statusCode %> - <%= title %></title>
</head>
<body>
   <h1><%= statusCode %> - <%= title %></h1>
   <p><%= Server.HtmlEncode(message) %></p>
   
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
</body>
</html>