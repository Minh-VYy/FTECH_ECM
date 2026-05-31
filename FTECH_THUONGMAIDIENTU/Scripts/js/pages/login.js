let selectedRole = 'customer';
let selectedRedirect = '/';

const roleConfig = {
  'super-admin': {
    icon: '⚙️',
    title: 'Xin chào, Super Admin!',
    desc: 'Bạn đã được cấp quyền toàn hệ thống để giám sát, duyệt nội dung và quản lý tài khoản quản trị.',
    role: 'Super Admin',
    href: '/Admin/Dashboard'
  },
  'content-manager': {
    icon: '👤',
    title: 'Xin chào, Content Manager!',
    desc: 'Bạn đã được cấp quyền truy cập khu vực Content để tạo, chỉnh sửa và quản lý bài viết.',
    role: 'Content Manager',
    href: '/ContentManager/Post'
  },
  'affiliate-manager': {
    icon: '🤝',
    title: 'Xin chào, Affiliate Manager!',
    desc: 'Bạn đã được cấp quyền truy cập khu vực Affiliate để quản lý đối tác, link và hiệu suất chuyển đổi.',
    role: 'Affiliate Manager',
    href: '/AffiliateManager/Dashboard'
  },
  'user-account-manager': {
    icon: '🛡️',
    title: 'Xin chào, User Account Manager!',
    desc: 'Bạn đã được cấp quyền xử lý tài khoản người dùng và các vi phạm liên quan.',
    role: 'User Account Manager',
    href: '/SuperAdmin/AdminAccount'
  },
  customer: {
    icon: '👤',
    title: 'Xin chào, Người dùng!',
    desc: 'Bạn có thể truy cập khu vực người dùng để đọc review, lưu nội dung và theo dõi hoạt động cá nhân.',
    role: 'Người dùng',
    href: '/'
  }
};

function setDemo(button, role) {
  selectedRole = role;
  document.querySelectorAll('.demo-role').forEach(el => el.classList.remove('active'));
  button.classList.add('active');
}

function togglePw() {
  const input = document.getElementById('loginPassword');
  input.type = input.type === 'password' ? 'text' : 'password';
}

function socialAuth(provider) {
  const toast = document.getElementById('toastSuccess');
  const error = document.getElementById('toastError');
  error.classList.remove('show');
  toast.textContent = `Đăng nhập bằng ${provider} đang được mô phỏng. Ở bản đầy đủ, hệ thống sẽ mở luồng xác thực và gán quyền truy cập theo ${provider}.`;
  toast.classList.add('show');
}

async function postForm(url, data) {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
      'X-Requested-With': 'XMLHttpRequest'
    },
    body: data.toString()
  });

  return response.json();
}

document.getElementById('loginForm').addEventListener('submit', async function (e) {
  e.preventDefault();

  const identifier = document.getElementById('identifier').value.trim();
  const password = document.getElementById('loginPassword').value.trim();
  const rememberMe = document.querySelector('input[name="rememberMe"]')?.checked || false;
  const errIdentifier = document.getElementById('errIdentifier');
  const errPassword = document.getElementById('errPassword');
  const toast = document.getElementById('toastSuccess');
  const error = document.getElementById('toastError');
  let invalid = false;

  errIdentifier.classList.remove('show');
  errPassword.classList.remove('show');
  toast.classList.remove('show');
  error.classList.remove('show');

  if (!identifier) {
    errIdentifier.textContent = 'Vui lòng nhập tài khoản hoặc email.';
    errIdentifier.classList.add('show');
    invalid = true;
  }

  if (!password) {
    errPassword.textContent = 'Vui lòng nhập mật khẩu.';
    errPassword.classList.add('show');
    invalid = true;
  }

  if (invalid) {
    error.classList.add('show');
    return;
  }

  const btn = document.getElementById('loginBtn');
  btn.disabled = true;
  btn.textContent = 'Đang xác thực...';

  try {
    const endpoint = document.getElementById('loginForm').action || '/login.html';
    const result = await postForm(endpoint, new URLSearchParams({
      Identifier: identifier,
      Password: password,
      RememberMe: String(rememberMe)
    }));

    if (!result.success) {
      error.textContent = result.message || 'Đăng nhập không thành công.';
      error.classList.add('show');
      btn.disabled = false;
      btn.textContent = 'Đăng nhập →';
      return;
    }

    const cfg = roleConfig[result.roleKey] || roleConfig[selectedRole] || roleConfig.customer;
    selectedRole = result.roleKey || selectedRole;
    selectedRedirect = result.redirectUrl || cfg.href;
    btn.textContent = '✅ Thành công!';
    toast.textContent = result.message || 'Xác thực thành công. Hệ thống đang điều hướng theo quyền truy cập của bạn.';
    toast.classList.add('show');
    showRoleModal(result.roleKey || selectedRole, result.displayName);
  } catch (ex) {
    error.textContent = 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
    error.classList.add('show');
    btn.disabled = false;
    btn.textContent = 'Đăng nhập →';
  }
});

function showRoleModal(role, displayName) {
  const cfg = roleConfig[role] || roleConfig.customer;
  selectedRedirect = cfg.href;
  document.getElementById('modalIcon').textContent = cfg.icon;
  document.getElementById('modalTitle').textContent = displayName ? `Xin chào, ${displayName}!` : cfg.title;
  document.getElementById('modalDesc').textContent = cfg.desc;
  document.getElementById('modalRole').textContent = cfg.role;
  document.getElementById('roleModal').classList.add('show');
}

function closeModal() {
  window.location.href = selectedRedirect;
}
