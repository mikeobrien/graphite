function navigate()
{
    var navElements = document.getElementsByClassName('menu-nav');
    [].forEach.call(navElements,
        function (e)
        {
            var anchor = e.getElementsByTagName('a')[0];
            if ((!window.location.hash && e == navElements[0]) ||
                    anchor.hash == window.location.hash)
                e.classList.add('selected');
            else e.classList.remove('selected');
        });

    var panels = document.getElementsByClassName('menu-content');
    [].forEach.call(panels,
        function (e) {
            if ((!window.location.hash && e == panels[0]) ||
                e.id == window.location.hash.replace('#', ''))
                e.classList.remove('hidden');
            else e.classList.add('hidden');
        });
}

function toggleVisibility(element)
{
    var nextElement = element.nextElementSibling;
    if (nextElement.style.display === 'none')
        nextElement.style.display = '';
    else nextElement.style.display = 'none';
}

navigate();

document.addEventListener('DOMContentLoaded', function ()
{
    [].forEach.call(document.getElementsByClassName('menu-nav'),
        function (e) {
            e.addEventListener('mouseover', function () {
                e.classList.add('hover');
            });
            e.addEventListener('mouseout', function () {
                e.classList.remove('hover');
            });
        });
    window.addEventListener('hashchange', navigate);

    document.getElementById('action-search')
        .addEventListener('input', function (s) {
            var search = s.target.value.toLowerCase();
            [].forEach.call(document.getElementsByClassName('action-row'),
                function (e) {
                    if (!search || e.getAttribute('data-description')
                            .toLowerCase().indexOf(search) > -1)
                        e.style.display = '';
                    else e.style.display = 'none'
                });
        })
});
