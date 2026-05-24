function partnerClick(name) {
  alert("Dieu huong den doi tac: " + name + ". Day la vi tri hop ly de mo affiliate link trong ban demo.");
}

function submitComment() {
  const input = document.getElementById("commentInput");
  const value = input.value.trim();

  if (!value) {
    alert("Vui long nhap noi dung binh luan.");
    return;
  }

  const block = document.createElement("div");
  const head = document.createElement("div");
  const name = document.createElement("div");
  const date = document.createElement("div");
  const text = document.createElement("div");

  block.className = "comment-item";
  head.className = "comment-head";
  name.className = "comment-name";
  date.className = "comment-date";
  text.className = "comment-text";

  name.textContent = "Nguyen Minh Khoa";
  date.textContent = "Vua xong";
  text.textContent = value;

  head.append(name, date);
  block.append(head, text);

  document.getElementById("commentList").prepend(block);
  input.value = "";
}
