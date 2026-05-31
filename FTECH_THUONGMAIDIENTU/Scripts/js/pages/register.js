const pwInput = document.getElementById('password');
if (pwInput) {
  pwInput.addEventListener('input', updateStrength);
}

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
  const ids = ['bar1', 'bar2', 'bar3', 'bar4'];
  const labels = ['Nhập mật khẩu để xem độ mạnh.', 'Mật khẩu yếu.', 'Mật khẩu tạm ổn.', 'Mật khẩu khá tốt.', 'Mật khẩu mạnh.'];
  ids.forEach((id, index) => {
    const el = document.getElementById(id);
    if (!el) {
      return;
    }
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
  if (!el) {
    return;
  }
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

document.getElementById('registerForm').addEventListener('submit', async function (e) {
  e.preventDefault();
  hideErrors();

  const lastName = document.getElementById('lastName').value.trim();
  const firstName = document.getElementById('firstName').value.trim();
  const email = document.getElementById('email').value.trim();
  const phoneInput = document.getElementById('phone');
  const phone = phoneInput ? phoneInput.value.trim().replace(/\s/g, '') : '';
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
  }

  if (phoneInput && phone && !/^[0-9]{9,11}$/.test(phone)) {
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
    const termsError = document.getElementById('errTerms');
    if (termsError) {
      termsError.classList.add('show');
    }
    invalid = true;
  }

  if (invalid) {
    document.getElementById('toastError').classList.add('show');
    return;
  }

  const btn = document.getElementById('submitBtn');
  btn.disabled = true;
  btn.textContent = 'Đang xử lý...';

  try {
    const result = await postForm('/Account/Register', new URLSearchParams({
      LastName: lastName,
      FirstName: firstName,
      Email: email,
      Username: username,
      Phone: phone,
      Password: password,
      ConfirmPassword: confirmPassword,
      TermsAccepted: String(terms)
    }));

    if (!result.success) {
      document.getElementById('toastError').textContent = result.message || 'Đăng ký không thành công.';
      document.getElementById('toastError').classList.add('show');
      btn.disabled = false;
      btn.textContent = 'Tạo tài khoản →';
      return;
    }

    document.getElementById('toastSuccess').textContent = result.message || 'Đăng ký thành công.';
    document.getElementById('toastSuccess').classList.add('show');
    document.getElementById('verifyBox').classList.add('show');
    btn.textContent = '✅ Đăng ký thành công!';
    btn.disabled = false;

    if (result.redirectUrl) {
      window.setTimeout(() => {
        window.location.href = result.redirectUrl;
      }, 900);
    }
  } catch (ex) {
    document.getElementById('toastError').textContent = 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
    document.getElementById('toastError').classList.add('show');
    btn.disabled = false;
    btn.textContent = 'Tạo tài khoản →';
  }
});
