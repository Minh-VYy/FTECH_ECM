(function () {
  const TOKEN_KEYS = [
    'ftech_access_token',
    'ftech_refresh_token',
    'accessToken',
    'refreshToken',
    'token',
    'role'
  ];

  function clearAuthStorage() {
    TOKEN_KEYS.forEach((key) => {
      localStorage.removeItem(key);
      sessionStorage.removeItem(key);
    });
  }

  function logout() {
    clearAuthStorage();
    window.location.href = '/Account/Login';
  }

  function ensureAdminLogoutButton() {
    const sidebarBottom = document.querySelector('.sidebar .sb-bottom');

    if (!sidebarBottom || sidebarBottom.querySelector('[data-logout]')) {
      return;
    }

    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'sb-logout';
    button.dataset.logout = 'true';
    button.textContent = 'Đăng xuất';
    sidebarBottom.appendChild(button);
  }

  document.addEventListener('DOMContentLoaded', () => {
    ensureAdminLogoutButton();

    document.querySelectorAll('[data-logout]').forEach((element) => {
      element.addEventListener('click', (event) => {
        event.preventDefault();
        logout();
      });
    });
  });

  window.FTECHAuth = {
    logout,
    clearAuthStorage
  };
})();
