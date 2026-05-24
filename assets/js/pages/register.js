const pwInput = document.getElementById('password');
    pwInput.addEventListener('input', updateStrength);

    function togglePw(id, button) {
      const input = document.getElementById(id);
      input.type = input.type === 'password' ? 'text' : 'password';
      button.textContent = input.type === 'password' ? '👁' : '🙈';
    }

    function updateStrength() {
      const v = pwInput.value;
      let score = 0;
      if (v.length >= 8) score++;
      if (/[A-Z]/.test(v)) score++;
      if (/[0-9]/.test(v)) score++;
      if (/[^a-zA-Z0-9]/.test(v)) score++;
      const ids = ['bar1','bar2','bar3','bar4'];
      const labels = ['Nhập mật khẩu để xem độ mạnh.','Mật khẩu yếu.','Mật khẩu tạm ổn.','Mật khẩu khá tốt.','Mật khẩu mạnh.'];
      ids.forEach((id, index) => {
        const el = document.getElementById(id);
        el.className = 'pw-bar';
        if (index < score) {
          el.classList.add('on');
          el.classList.add(score >= 4 ? 'strong' : score >= 2 ? 'fair' : 'weak');
        }
      });
      document.getElementById('pwLabel').textContent = labels[score];
    }

    function showError(id, message) {
      const el = document.getElementById(id);
      el.textContent = message;
      el.classList.add('show');
    }

    function hideErrors() {
      document.querySelectorAll('.field-error, .check-error, .toast, .toast-error, .verify-box').forEach(el => {
        el.classList.remove('show');
      });
    }

    function socialAuth(provider) {
      hideErrors();
      const toast = document.getElementById('toastSuccess');
      toast.textContent = `Đăng ký bằng ${provider} đang được mô phỏng. Ở bản đầy đủ, hệ thống sẽ mở luồng xác thực riêng của ${provider}.`;
      toast.classList.add('show');
    }

    document.getElementById('registerForm').addEventListener('submit', function (e) {
      e.preventDefault();
      hideErrors();

      const lastName = document.getElementById('lastName').value.trim();
      const firstName = document.getElementById('firstName').value.trim();
      const email = document.getElementById('email').value.trim();
      const phone = document.getElementById('phone').value.trim().replace(/\s/g, '');
      const username = document.getElementById('username').value.trim();
      const password = document.getElementById('password').value;
      const confirmPassword = document.getElementById('confirmPassword').value;
      const terms = document.getElementById('terms').checked;
      let invalid = false;

      if (!lastName) { showError('errLastName', 'Vui lòng nhập họ.'); invalid = true; }
      if (!firstName) { showError('errFirstName', 'Vui lòng nhập tên.'); invalid = true; }

      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        showError('errEmail', 'Email không hợp lệ.');
        invalid = true;
      } else if (email.toLowerCase() === 'taken@ftech.vn' || email.toLowerCase() === 'existing@email.com') {
        showError('errEmail', 'Email đã tồn tại. Vui lòng dùng email khác.');
        invalid = true;
      }

      if (!/^[0-9]{9,11}$/.test(phone)) {
        showError('errPhone', 'Số điện thoại không hợp lệ.');
        invalid = true;
      }

      if (!/^[a-zA-Z0-9_]{4,}$/.test(username)) {
        showError('errUsername', 'Tên đăng nhập tối thiểu 4 ký tự và chỉ gồm chữ, số, dấu _.');
        invalid = true;
      }

      if (password.length < 8) {
        showError('errPassword', 'Mật khẩu phải có ít nhất 8 ký tự.');
        invalid = true;
      }

      if (!confirmPassword || confirmPassword !== password) {
        showError('errConfirmPassword', 'Mật khẩu xác nhận không khớp.');
        invalid = true;
      }

      if (!terms) {
        document.getElementById('errTerms').classList.add('show');
        invalid = true;
      }

      if (invalid) {
        document.getElementById('toastError').classList.add('show');
        return;
      }

      const btn = document.getElementById('submitBtn');
      btn.disabled = true;
      btn.textContent = 'Đang xử lý...';

      setTimeout(() => {
        document.getElementById('toastSuccess').classList.add('show');
        document.getElementById('verifyBox').classList.add('show');
        btn.textContent = '✅ Đăng ký thành công!';
        btn.disabled = false;
      }, 900);
    });
