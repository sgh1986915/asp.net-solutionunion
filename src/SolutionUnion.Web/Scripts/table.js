/// <reference path="jquery-1.6.2-vsdoc.js" />

(function (/*$*/) {

   /* util
   ---------------------------------*/
   function QueryString(qs) {

      this.params = new Object();
      this.get = function (key, default_) {

         // This silly looking line changes UNDEFINED to NULL
         if (default_ == null) default_ = null;

         var value = this.params[key];
         if (value == null) value = default_;

         return value;
      };
      this.set = function (key, val) {
         this.params[key] = val;
      };
      this.remove = function (key) {
         if (this.params[key])
            delete this.params[key];
      };

      if (qs == null)
         qs = window.location.search.substring(1, window.location.search.length);

      if (qs.length > 0) {

         // Turn <plus> back to <space>
         // See: http://www.w3.org/TR/REC-html40/interact/forms.html#h-17.13.4.1
         qs = qs.replace(/\+/g, ' ');
         var args = qs.split('&');

         for (var i = 0; i < args.length; i++) {
            var pair = args[i].split('=');
            var name = decodeURIComponent(pair[0]);
            var value = (pair.length == 2) ? decodeURIComponent(pair[1]) : null;

            this.params[name] = value;
         }
      }
   }

   /* sortable
   ---------------------------------*/
   $.fn.sortable = function (options) {

      var settings = $.extend({
         currentValue: null,
         sortParam: 'sort',
         startIndexParam: 'start',
         click: function (value, query) {
            window.location.href = query;
         }
      }, options);

      var qs = new QueryString();

      var current = {
         name: settings.currentValue || qs.get(settings.sortParam) || '',
         dir: 'asc',
         init: function () {

            if (this.name) {

               var parts = this.name.split(/\s/);

               if (parts.length > 1) {
                  var lastPart = parts[parts.length - 1].toLowerCase();

                  if (lastPart == 'asc' || lastPart == 'desc') {
                     this.dir = lastPart;
                     parts.pop();
                  }

                  this.name = parts.join(' ');
               }
            }
         },
         changeTo: function (n) {

            if (this.name.toLowerCase() == n.toLowerCase())
               this.dir = (this.dir == 'asc') ? 'desc' : 'asc';
            else
               this.dir = 'asc';

            this.name = n;
         },
         getParam: function () {

            var p = this.name;

            if (p && this.dir != 'asc')
               p += ' ' + this.dir;

            return p;
         }
      };

      var getName = function ($el) {
         
         var n = $el.data('name');

         if (n == '')
            return n;

         return n || $.trim($el.text());
      };

      var sort = function () {

         var $this = $(this);
         var name = getName($this);

         current.changeTo(name);
         var param = current.getParam();

         qs.remove(settings.startIndexParam);
         qs.set(settings.sortParam, param);

         settings.click(param, '?' + $.param(qs.params));
      };

      current.init();

      var $headers;
      var isTable = this.is('table');

      if (isTable)
         $headers = this.find('thead th');
      else 
         $headers = this.children();

      $headers = $headers.filter(function () { return getName($(this)) != ''; });

      $headers
         .addClass('clickable')
         .click(sort);

      if (current.name) {
         $headers
            .filter(function () { return getName($(this)) == current.name; })
            .addClass('sorting');
      }

      return this;
   };

   /* queryable
   ---------------------------------*/
   $.fn.queryable = function (options) {

      var settings = $.extend({
         requiredParams: ['limit', 'sort'],
         filtersInsideGridSelector: 'thead tr.filters :input',
         filtersOutsideGridSelector: ':input.filter',
         clearButtonSelector: 'thead tr.filters .clearLink'
      }, options);

      var currentQuery = new QueryString();
      var refreshQuery = {};

      $.each(settings.requiredParams, function (i, key) {
         var currentVal = currentQuery.get(key);

         if (currentVal)
            refreshQuery[key] = currentVal;
      });

      var refresh = function () {

         var queryString = $.param(refreshQuery);

         var url = (queryString) ?
            "?" + queryString
            : window.location.href.substring(0, window.location.href.length - window.location.search.length);

         window.location.href = url;
      };

      var clear = function () {
         refresh();
      };

      var plugin = function () {
         var $this = $(this);

         var controls = $(settings.filtersInsideGridSelector, $this).add($(settings.filtersOutsideGridSelector));

         var onchange = function (e) {
            controls.each(function (i) {
               var $this = $(this);
               if ($this.val())
                  refreshQuery[this.name] = $this.val();
            });

            refresh();
         };

         controls.each(function () {
            if (this.tagName.toLowerCase() == 'select') {
               $(this).change(onchange);
            } else {
               $(this).keyup(function (e) {
                  if (e.keyCode == 13)
                     onchange(e);
               });
            }
         });

         $(settings.clearButtonSelector, $this).click(clear);
      };

      return this.each(plugin);
   };


   /* editable
   ---------------------------------*/
   $.fn.editable = function (options) {

      var settings = $.extend({}, options);

      var plugin = function () {

         var $table = $(this);
         var $headers = $('thead th', $table);
         var $dataHeaders = $headers.filter(function () { return $(this).data('name') != null; });
         var $keyHeader = $dataHeaders.first();
         var $editableIndexes = $dataHeaders.map(function () { return $(this).index(); });

         /* Updating
         ---------------------------------*/
         var cellClick = function (e) {

            e.stopImmediatePropagation();

            var $this = $(this);
            $this.unbind('click', cellClick);

            var value = $.trim($this.text());

            $this.empty();

            $('<input type="text"/>')
               .attr('name', $headers.eq($this.index()).data('name'))
               .val(value)
               .attr('size', (value.length > 0) ? value.length : 10)
               .blur(inputBlur)
               .keyup(inputKeyUp)
               .appendTo($this)
               .focus();
         };

         var inputBlur = function (e) {

            var $this = $(this);
            var $td = $this.closest('td');
            var originalVal = $td.data('value');

            $this.remove();
            $td.text(originalVal)
               .click(cellClick);
         };

         var inputKeyUp = function (e) {

            var $this = $(this);
            var $td = $this.closest('td');

            if (e.keyCode == 13) {

               $this.unbind('blur', inputBlur)
                  .attr('disabled', 'disabled');

               var val = $this.val();
               var name = $this.attr('name');
               var data = {};
               data[name] = val;

               var keyValue = $.trim($this.closest('tr').children().eq($keyHeader.index()).data('value'));

               $.ajax({
                  type: 'POST',
                  url: window.location.pathname + '/' + encodeURIComponent(keyValue),
                  data: data,
                  beforeSend: function (xhr) {
                     xhr.setRequestHeader('X-HTTP-Method-Override', 'PATCH');
                  },
                  error: function (xhr, status, error) {

                     alert(xhr.responseText);

                     $this.attr('disabled', '')
                        .blur(inputBlur)
                        .focus();
                  },
                  success: function (data, status, xhr) {

                     var newValues = new QueryString(data);

                     $td.data('value', newValues.get(name));
                     $this.blur(inputBlur)
                        .blur();
                  }
               });
            }
         };

         $('tbody tr', $table).each(function () {

            var $tr = $(this);
            var $cells = $('td', $tr);
            var $editableCells = $cells.filter(function () { return $.inArray($(this).index(), $editableIndexes) != -1; });

            $editableCells
               .each(function () {
                  $(this).data('value', $.trim($(this).text()));
               })
               .addClass('editable')
               .click(cellClick);
         });

         /* Inserting
         ---------------------------------*/
         var newInputKeyUp = function (e) {

            var $this = $(this);
            var $inputs = $(':input', $this.closest('tr'));

            if (e.keyCode == 13) {
               $inputs.attr('disabled', 'disabled');

               var data = {};

               $inputs.each(function () {
                  var $this = $(this);
                  data[$this.attr('name')] = $this.val()
               });

               $.ajax({
                  type: 'POST',
                  url: window.location.pathname,
                  data: data,
                  error: function (xhr, status, error) {

                     alert(xhr.responseText);

                     $inputs.attr('disabled', '');
                     $this.focus();
                  },
                  success: function (data, status, xhr) {

                     var newValues = new QueryString(data);

                     $inputs.val('').attr('disabled', '');

                     var $createdTr = $('<tr/>');

                     $headers.each(function () {

                        var name = $(this).data('name');
                        var $newCell = $('<td/>');

                        if (name != null)
                           $newCell.text(newValues.get(name))

                        $createdTr.append($newCell);
                     });

                     $('tbody', $table).append($createdTr);
                  }
               });
            }
         };

         var $newTr = $('<tr/>');

         $headers.each(function () {

            var $newCell = $('<td/>');
            var name = $(this).data('name');

            if (name != null)
               $('<input type="text"/>')
                  .attr('name', name)
                  .keyup(newInputKeyUp)
                  .appendTo($newCell);

            $newTr.append($newCell);
         });

         $('tfoot', $table).prepend($newTr);

         /* Deleting
         ---------------------------------*/
         var delBtnClick = function (e) {

            var confirmed = confirm('Are you sure? (this action cannot be undone)');

            if (confirmed) {

               var $this = $(this);
               var keyValue = $.trim($this.closest('tr').children().eq($keyHeader.index()).data('value'));

               $this.unbind('click', delBtnClick);

               $.ajax({
                  type: 'POST',
                  url: window.location.pathname + '/' + encodeURIComponent(keyValue),
                  beforeSend: function (xhr) {
                     xhr.setRequestHeader('X-HTTP-Method-Override', 'DELETE');
                  },
                  error: function (xhr, status, error) {

                     alert(xhr.responseText);

                     $this.click(delBtnClick);
                  },
                  success: function (data, status, xhr) {

                     var $tr = $this.closest('tr');

                     $tr.children().animate({ backgroundColor: 'red', opacity: 0 }, 'slow', 'swing', function () {
                        $(this).slideUp(function () {
                           $tr.nextAll().toggleClass('odd');
                           $tr.remove();
                        });
                     });
                  }
               });
            }
         };

         //         $('thead tr', $table).each(function() {
         //            var $this = $(this);
         //            var tagName = $this.children().last()[0].tagName.toLowerCase();
         //            $this.append($('<' + tagName + '/>'));
         //         });

         //         $('tfoot tr', $table).append($('<td/>'));

         //         $('tbody tr', $table).each(function() {

         //            $(this).append(
         //               $('<td/>').append(
         //                  $('<img src="/Content/shared/16-circle-red-remove.png" class="del" alt="Delete" title="Delete"/>')
         //                     .click(delBtnClick)
         //               )
         //            );
         //         });
      };

      return this.each(plugin);
   };

})(jQuery);
