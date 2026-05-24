const toastSuccess = document.getElementById('toastSuccess');
    const toastError = document.getElementById('toastError');
    const emailInput = document.getElementById('email');

    function showToast(type, message) {
      toastSuccess.classList.remove('show');
      toastError.classList.remove('show');
      if (type === 'success') {
        toastSuccess.textContent = message;
        toastSuccess.classList.add('show');
      } else {
        toastError.textContent = message;
        toastError.classList.add('show');
      }
    }

    function setProgress(step) {
      document.getElementById('prog1').classList.toggle('active', step === 1);
      document.getElementById('prog2').classList.toggle('active', step >= 2);
      document.getElementById('prog3').classList.toggle('active', step >= 3);
    }

    function goStep2(isResend = false) {
      const value = emailInput.value.trim();
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        showToast('error', 'Email không hợp lệ. Vui lòng nhập đúng email đã đăng ký.');
        emailInput.focus();
        return;
      }
      document.getElementById('step1').classList.remove('active');
      document.getElementById('step2').classList.add('active');
      setProgress(2);
      document.getElementById('sentEmailText').textContent = `Chúng tôi đã gửi liên kết đặt lại mật khẩu đến ${value}.`;
      showToast('success', isResend ? 'Liên kết đặt lại mật khẩu đã được gửi lại.' : 'Email khôi phục đã được gửi. Vui lòng kiểm tra hộp thư của bạn.');
    }

    function backStep1() {
      document.getElementById('step2').classList.remove('active');
      document.getElementById('step1').classList.add('active');
      setProgress(1);
      toastSuccess.classList.remove('show');
      toastError.classList.remove('show');
    }

    function goStep3() {
      document.getElementById('step2').classList.remove('active');
      document.getElementById('step3').classList.add('active');
      setProgress(3);
      showToast('success', 'Vui lòng nhập mật khẩu mới và xác nhận lại.');
    }

    function backStep2() {
      document.getElementById('step3').classList.remove('active');
      document.getElementById('step2').classList.add('active');
      setProgress(2);
      toastSuccess.classList.remove('show');
      toastError.classList.remove('show');
    }

    function submitReset() {
      const pw = document.getElementById('newPassword').value;
      const confirm = document.getElementById('confirmPassword').value;
      if (pw.length < 8) {
        showToast('error', 'Mật khẩu mới phải có ít nhất 8 ký tự.');
        return;
      }
      if (pw !== confirm) {
        showToast('error', 'Mật khẩu xác nhận không khớp.');
        return;
      }
      document.getElementById('step3').classList.remove('active');
      document.getElementById('step4').classList.add('active');
      toastSuccess.classList.remove('show');
      toastError.classList.remove('show');
    }
