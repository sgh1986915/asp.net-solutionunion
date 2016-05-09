/// <reference path="jquery-1.6.2-vsdoc.js" />

// Prevent AJAX caching on IE
if ($.browser.msie) {
   $.ajaxSetup({
      cache: false
   });
}

function ShowMessage(msg, selector) {
   
   var $message = $(selector || '#message');
   $message.css({ display: '', opacity: 0 }).text(msg);

   $message.animate({ opacity: 1 });

   clearTimeout(this.timeout);

   this.timeout = setTimeout(function () { $message.fadeOut() }, 10000);
}

$(function () {

   if ($.browser.webkit)
      $('body').addClass('webkit');

   /* Menus
   --------------------------------------------------------*/
   var $last_active_menu_item = $("ul#lnb > li.active");
   var $last_active_submenu = $("ul#lnb > li.active > div");

   $("ul#lnb > li").click(function () {

      var $current_active_menu_item = $(this);
      var $current_active_submenu = $(this).children("div");
      var height = $("div > ul", this).outerHeight()

      if ($current_active_menu_item.attr("id") != $last_active_menu_item.attr("id")) {

         $(this).children("div").animate({
            height: height
         }, 'slow');

         if ($last_active_submenu != null) {
            $last_active_submenu.animate({
               height: "0"
            }, 'slow');
         }

         $last_active_menu_item = $(this);
         $last_active_submenu = $(this).children("div");
      }
   });

   $("ul#lnb > li > div > ul > li").click(function (e) {
      e.stopImmediatePropagation();
   });

   /* Hide Server Message
   --------------------------------------------------------*/
   setTimeout(function () { $('#message.hide-after-load').fadeOut(); }, 10000);

   /* Tooltips
   --------------------------------------------------------*/
   if ($.tooltip)
      $('.tooltip[title]').tooltip();

   if ($.fancybox)
      $('a.cvv').fancybox({ href: '/Help/CVV', type: 'iframe', width: 700, height: '80%' });

   /* btn
   --------------------------------------------------------*/
   $('a.btn').bind({
      focus: function () {
         $(this).addClass('focus');
      },
      blur: function () {
         $(this).removeClass('focus');
      },
      keypress: function (e) {

         if ((e.keyCode || e.which) == 13) {
            $(this).click();
            return false;
         }
      }
   });
});