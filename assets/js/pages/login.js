let selectedRole = 'customer';
    let selectedRedirect = 'trangchu.html';

    const roleConfig = {
      customer: {
        icon: '👤',
        title: 'Xin chào, Người dùng!',
        desc: 'Bạn có thể truy cập khu vực người dùng để đọc review, lưu nội dung và theo dõi hoạt động cá nhân.',
        role: 'Người dùng',
        href: 'trangchu.html'
      },
      content: {
        icon: '✍️',
        title: 'Xin chào, Content Manager!',
        desc: 'Bạn đã được cấp quyền truy cập khu vực Content để tạo, chỉnh sửa và quản lý bài viết.',
        role: 'Content Manager',
        href: 'content-manager.html'
      },
      partner: {
        icon: '🤝',
        title: 'Xin chào, Affiliate Manager!',
        desc: 'Bạn đã được cấp quyền truy cập khu vực Affiliate để quản lý đối tác, link và hiệu suất chuyển đổi.',
        role: 'Affiliate Manager',
        href: 'manage-affiliates.html'
      },
      admin: {
        icon: '⚙️',
        title: 'Xin chào, Super Admin!',
        desc: 'Bạn có toàn quyền giám sát hệ thống, duyệt nội dung, duyệt đối tác và quản lý tài khoản admin.',
        role: 'Super Admin',
        href: 'dashboard.html'
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

    document.getElementById('loginForm').addEventListener('submit', function (e) {
      e.preventDefault();

      const identifier = document.getElementById('identifier').value.trim();
      const password = document.getElementById('loginPassword').value.trim();
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
      } else if (identifier.toLowerCase() === 'unknown' || identifier.toLowerCase() === 'sai@email.com') {
        errIdentifier.textContent = 'Tài khoản hoặc email không tồn tại.';
        errIdentifier.classList.add('show');
        invalid = true;
      }

      if (!password) {
        errPassword.textContent = 'Vui lòng nhập mật khẩu.';
        errPassword.classList.add('show');
        invalid = true;
      } else if (password === '123' || password.toLowerCase() === 'sai') {
        errPassword.textContent = 'Mật khẩu không đúng.';
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

      setTimeout(() => {
        btn.textContent = '✅ Thành công!';
        toast.textContent = 'Xác thực thành công. Hệ thống đang điều hướng theo quyền truy cập của bạn.';
        toast.classList.add('show');
        showRoleModal(selectedRole);
      }, 900);
    });

    function showRoleModal(role) {
      const cfg = roleConfig[role];
      selectedRedirect = cfg.href;
      document.getElementById('modalIcon').textContent = cfg.icon;
      document.getElementById('modalTitle').textContent = cfg.title;
      document.getElementById('modalDesc').textContent = cfg.desc;
      document.getElementById('modalRole').textContent = cfg.role;
      document.getElementById('roleModal').classList.add('show');
    }

    function closeModal() {
      window.location.href = selectedRedirect;
    }
