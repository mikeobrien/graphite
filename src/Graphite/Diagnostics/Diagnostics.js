function navigate()
{
    var hash = window.location.hash || 'configuration';

    [].forEach.call(document.getElementsByClassName("menu-nav"),
        function (e)
        {
            var anchor = e.getElementsByTagName('a')[0];
            if (anchor.hash == window.location.hash)
                e.classList.add('selected');
            else e.classList.remove('selected');
        });

    [].forEach.call(document.getElementsByClassName("menu-content"),
        function(e) {
            e.classList.add('hidden');
        });

    document.getElementById(hash.replace('#', '')).classList.remove('hidden');
}

function toggleVisibility(element)
{
    var nextElement = element.nextElementSibling;
    if (nextElement.style.display === 'none') {
        {
            nextElement.style.display = '';
        }
    } else {
        {
            nextElement.style.display = 'none';
        }
    }
}

navigate();

document.addEventListener("DOMContentLoaded", function ()
{
    [].forEach.call(document.getElementsByClassName("menu-nav"),
        function (e) {
            e.addEventListener("mouseover", function () {
                e.classList.add('hover');
            });
            e.addEventListener("mouseout", function () {
                e.classList.remove('hover');
            });
        });
    window.addEventListener('hashchange', navigate);
});
