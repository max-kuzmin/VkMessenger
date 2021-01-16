namespace ru.MaxKuzmin.VkMessenger
{
    public static class AuthorizationPageScript
    {
        public const string Script = @"
function hideByClassName(className) {
    var elems = document.getElementsByClassName(className);
    if (elems.length > 0) elems[0].style.display = 'none';
}

function setWhiteBackgroundByClassName(className) {
    var elems = document.getElementsByClassName(className);
    if (elems.length > 0) elems[0].style.backgroundColor = 'white';
}

function onLoad() {
    //Replace content of page after authorization with custom text
    if (document.getElementsByClassName('button').length === 0) {
        document.body.innerText = '{PleaseWait}';
        document.body.style.textAlign = 'center';
        document.body.style.paddingTop = '150px';
    }

    //For all pages hide some elements and set background
    hideByClassName('mh_btn_label');
    hideByClassName('near_btn');
    hideByClassName('fi_header fi_header_light');
    hideByClassName('button wide_button gray_button');

    setWhiteBackgroundByClassName('basis__content mcont');
    setWhiteBackgroundByClassName('vk__page');

    document.body.style.marginLeft = '50px';
    document.body.style.marginRight = '50px';
    document.body.style.backgroundColor = 'white';
}

window.addEventListener('load', () => onLoad());
onLoad();
";
    }
}
