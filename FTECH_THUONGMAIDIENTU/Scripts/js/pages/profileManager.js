function sw(id) {
      document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
      document.getElementById('tab-' + id).classList.add('active');
      const order = ['info','orders','reviews','comments','saved','wishlist','addresses','password','notifications'];
      document.querySelectorAll('.menu button').forEach((btn, index) => {
        btn.classList.toggle('active', order[index] === id);
      });
    }

    let editing = false;
    function toggleEdit() {
      editing = !editing;
      document.querySelectorAll('#tab-info input, #tab-info select, #tab-info textarea').forEach(el => {
        el.disabled = !editing;
      });
      document.getElementById('saveWrap').style.display = editing ? 'block' : 'none';
      document.querySelector('.edit-btn').textContent = editing ? 'Hủy' : 'Chỉnh sửa';
    }

    function saveInfo() {
      toggleEdit();
      alert('Đã cập nhật thông tin cá nhân.');
    }

    function changePw() {
      const oldPw = document.getElementById('oldPw').value.trim();
      const newPw = document.getElementById('newPw').value.trim();
      const confirmPw = document.getElementById('confirmPw').value.trim();
      if (!oldPw || !newPw || !confirmPw) {
        alert('Vui lòng điền đầy đủ các trường mật khẩu.');
        return;
      }
      if (newPw.length < 8) {
        alert('Mật khẩu mới phải có ít nhất 8 ký tự.');
        return;
      }
      if (newPw !== confirmPw) {
        alert('Xác nhận mật khẩu không khớp.');
        return;
      }
      alert('Đổi mật khẩu thành công.');
      document.getElementById('oldPw').value = '';
      document.getElementById('newPw').value = '';
      document.getElementById('confirmPw').value = '';
    }

    document.addEventListener('DOMContentLoaded', () => {
      const tabId = window.location.hash.replace('#tab-', '');
      if (tabId && document.getElementById('tab-' + tabId)) {
        sw(tabId);
      }
    });
