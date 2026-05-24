(function () {
  const includeNodes = document.querySelectorAll("[data-include]");

  includeNodes.forEach(async (node) => {
    const source = node.getAttribute("data-include");

    try {
      const response = await fetch(source);

      if (!response.ok) {
        throw new Error(`Khong tai duoc component: ${source}`);
      }

      node.innerHTML = await response.text();
      node.dispatchEvent(new CustomEvent("component:loaded", { bubbles: true }));
    } catch (error) {
      console.error(error);
      node.innerHTML = "";
    }
  });
})();
