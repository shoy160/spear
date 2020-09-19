(function ($) {
    //console.log('enjoy');
    $(function () {
        var url = window.location.search.match(/url=([^&]+)/);
        if (url && url.length > 1) {
            url = decodeURIComponent(url[1]);
        } else {
            url = undefined;
        }
        //重置样式
        $('.logo__title').html('I车保文档');
        $('.logo__img').attr('src', 'http://file.i-cbao.com//uploads/icb.png');

        var $input = $('#input_baseUrl');
        var $routes = $('<select class="routes">');
        $routes.bind('change',
            function () {
                var $route = $routes.find('option:selected');
                location.href = location.origin + location.pathname + '?url=' + encodeURIComponent($route.val());
            });
        $.get('/swagger/routes',
            function (routes) {
                for (var i = 0; i < routes.length; i++) {
                    var route = routes[i];
                    var $option = $('<option value="' + route.url + '">' + route.title + '</option>');
                    if (url === route.url) {
                        $('title').html(route.title);
                        $option.attr('selected', 'selected');
                    }
                    $routes.append($option);
                }
                $input.after($routes);
            });
    });
})(jQuery);