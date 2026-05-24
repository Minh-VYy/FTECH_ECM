# FTECH Frontend Structure

## Thu muc dung chung

- `components/`: chua cac khoi HTML co the tai lai bang `assets/js/common/include.js`.
- `assets/css/common/`: CSS nen tang, layout dung chung va utility tach tu inline style cu.
- `assets/css/pages/`: CSS rieng cua tung trang, dat ten theo file HTML tuong ung.
- `assets/js/common/`: JavaScript dung chung cho nhieu trang.
- `assets/js/pages/`: JavaScript rieng cua tung trang, dat ten theo file HTML tuong ung.
- `assets/images/`: hinh anh, SVG va cac tai nguyen media.

## Quy uoc them trang moi

1. Tao file HTML o thu muc goc neu van can giu link hien tai.
2. Dat CSS rieng tai `assets/css/pages/<ten-trang>.css`.
3. Dat JS rieng tai `assets/js/pages/<ten-trang>.js`.
4. Neu co header/footer hoac khoi UI lap lai, tach sang `components/` va goi bang:

```html
<div data-include="components/header.html"></div>
<script src="assets/js/common/include.js"></script>
```

## Luu y van hanh

Component include dung `fetch`, nen chay trang qua Live Server hoac local server thay vi mo truc tiep bang `file://`.

## Dang nhap va dang xuat

- `assets/js/common/auth.js` la diem dung chung cho thao tac dang xuat va xoa token demo.
- Cac nut dang xuat nen dung thuoc tinh `data-logout` de backend co the ket noi token/session ve sau ma khong phai sua tung trang.
- Cac trang admin dung `assets/css/common/admin-shell.css` de hien thi nut dang xuat trong sidebar.
