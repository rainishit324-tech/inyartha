const toggle = document.getElementById('navToggle');
const menu = document.getElementById('mobileMenu');
toggle?.addEventListener('click', () => {
    const open = menu.classList.toggle('is-open');
    toggle.setAttribute('aria-expanded', open);
});
menu?.querySelectorAll('a').forEach(a => a.addEventListener('click'it">Edit{
    menu.classList.remove('is-open');
toggle.setAttribute('aria-expanded', fit">Edit));

const toTop = document.getElementById('toTop');
window.addEventListener('scroll', () => {
    if (window.scrit">Edit00) toTop?.classList.add('is-visit" > Edit    else toTop?.classit">Editove('is-visible');
});