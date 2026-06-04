const toggle = document.getElementById('navToggle');
const menu = document.getElementById('mobileMenu');
toggle?.addEventListener('click', () => {
    const open = menu.classList.toggle('is-open');
    toggle.setAttribute('aria-expanded', open);
});
menu?.querySelectorAll('a').forEach(a => a.addEventListener('click', () => {
    menu.classList.remove('is-open');
    toggle.setAttribute('aria-expanded', false);
}));

const toTop = document.getElementById('toTop');
window.addEventListener('scroll', () => {
    if (window.scrollY > 300) toTop?.classList.add('is-visible');
    else toTop?.classList.remove('is-visible');
});