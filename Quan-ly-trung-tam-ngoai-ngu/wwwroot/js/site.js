document.addEventListener("DOMContentLoaded", () => {
  const isDashboardShell = document.body.classList.contains("dashboard-shell");
  const confirmModalElement = document.getElementById("confirmActionModal");
  const confirmMessageElement = document.getElementById("confirmActionMessage");
  const confirmButtonElement = document.getElementById("confirmActionButton");
  const confirmModal = confirmModalElement ? new bootstrap.Modal(confirmModalElement) : null;

  document.querySelectorAll(".confirm-action").forEach((trigger) => {
    trigger.addEventListener("click", (event) => {
      event.preventDefault();
      if (!confirmModal || !confirmMessageElement || !confirmButtonElement) return;

      confirmMessageElement.textContent =
        trigger.getAttribute("data-confirm-message") || "Bạn có chắc chắn muốn tiếp tục?";

      confirmButtonElement.onclick = () => {
        confirmModal.hide();
        const targetUrl = trigger.getAttribute("data-target-url") || trigger.getAttribute("href");
        if (targetUrl && targetUrl !== "#") {
          window.location.href = targetUrl;
          return;
        }

        showInlineToast("Đã xác nhận thao tác thành công.", "success");
      };

      confirmModal.show();
    });
  });

  initializeManagementSearch();
  initializeHomeScrollNav();

  document.querySelectorAll(".app-chart").forEach((canvas) => {
    const raw = canvas.getAttribute("data-chart-config");
    if (!raw) return;

    const config = JSON.parse(raw);
    const colors = config.Colors?.length ? config.Colors : ["#446a9f"];
    const labelColor = isDashboardShell ? "#f8fbff" : "#314667";
    const mutedColor = isDashboardShell ? "rgba(248, 251, 255, 0.78)" : "#7b8aa2";
    const gridColor = isDashboardShell ? "rgba(255, 255, 255, 0.12)" : "rgba(217, 226, 238, 0.9)";

    new Chart(canvas, {
      type: config.ChartType || "bar",
      data: {
        labels: config.Labels,
        datasets: [{
          label: config.Title,
          data: config.Values,
          borderColor: colors[0],
          backgroundColor: config.ChartType === "line"
            ? (isDashboardShell ? "rgba(248, 251, 255, 0.12)" : "rgba(68, 106, 159, 0.14)")
            : colors,
          fill: config.ChartType === "line",
          borderWidth: 2,
          borderRadius: 12,
          tension: 0.35
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: config.ChartType === "doughnut",
            labels: {
              color: labelColor
            }
          },
          tooltip: { mode: "index", intersect: false }
        },
        scales: config.ChartType === "doughnut" ? {} : {
          x: {
            grid: { display: false, color: gridColor },
            ticks: { color: mutedColor }
          },
          y: {
            beginAtZero: true,
            ticks: { precision: 0, color: mutedColor },
            grid: { color: gridColor }
          }
        }
      }
    });
  });
});

function initializeManagementSearch() {
  const normalizeText = (value) => value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();

  document.querySelectorAll("[data-management-list]").forEach((container) => {
    const searchInput = container.querySelector("[data-management-search]");
    const rows = Array.from(container.querySelectorAll("[data-management-row]"));
    const emptyRow = container.querySelector("[data-management-search-empty]");
    const counter = container.querySelector("[data-management-count]");

    if (!counter) {
      return;
    }

    const updateCounter = (visibleCount, totalCount, hasSearch) => {
      if (!totalCount) {
        counter.textContent = "Chưa có bản ghi nào để hiển thị.";
        return;
      }

      counter.textContent = hasSearch
        ? `Đang hiển thị ${visibleCount}/${totalCount} bản ghi khớp từ khóa.`
        : `Đang hiển thị ${totalCount} bản ghi trên một trang.`;
    };

    if (!searchInput || !rows.length) {
      updateCounter(rows.length, rows.length, false);
      return;
    }

    const applySearch = () => {
      const keyword = normalizeText(searchInput.value.trim());
      let visibleCount = 0;

      rows.forEach((row) => {
        const matches = !keyword || normalizeText(row.textContent || "").includes(keyword);
        row.classList.toggle("d-none", !matches);
        if (matches) {
          visibleCount += 1;
        }
      });

      if (emptyRow) {
        emptyRow.classList.toggle("d-none", visibleCount !== 0);
      }

      updateCounter(visibleCount, rows.length, keyword.length > 0);
    };

    searchInput.addEventListener("input", applySearch);
    applySearch();
  });
}

function initializeHomeScrollNav() {
  const body = document.body;
  if (!body || body.getAttribute("data-controller") !== "Home") {
    return;
  }

  const links = Array.from(document.querySelectorAll("[data-scroll-nav]"));
  const sections = links
    .map((link) => document.getElementById(link.getAttribute("data-scroll-nav") || ""))
    .filter(Boolean);

  const collapseElement = document.getElementById("mainNav");
  const navCollapse = collapseElement ? bootstrap.Collapse.getOrCreateInstance(collapseElement, { toggle: false }) : null;

  links.forEach((link) => {
    link.addEventListener("click", () => {
      if (window.innerWidth < 992 && navCollapse) {
        navCollapse.hide();
      }
    });
  });

  if (!sections.length) {
    return;
  }

  const setActive = (id) => {
    links.forEach((link) => {
      link.classList.toggle("active", link.getAttribute("data-scroll-nav") === id);
    });
  };

  const observer = new IntersectionObserver((entries) => {
    const visibleSection = entries
      .filter((entry) => entry.isIntersecting)
      .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

    if (visibleSection?.target?.id) {
      setActive(visibleSection.target.id);
    }
  }, {
    rootMargin: "-25% 0px -55% 0px",
    threshold: [0.2, 0.35, 0.55]
  });

  sections.forEach((section) => observer.observe(section));

  if (window.location.hash) {
    setActive(window.location.hash.replace("#", ""));
  } else {
    setActive("trang-chu");
  }
}

function showInlineToast(message, type) {
  const toastContainer = document.createElement("div");
  toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3";
  toastContainer.innerHTML = `
    <div class="toast show border-0 app-toast app-toast-${type}" role="alert">
      <div class="toast-header border-0">
        <strong class="me-auto">Thông báo</strong>
        <small>vừa xong</small>
        <button type="button" class="btn-close ms-2 mb-1" data-bs-dismiss="toast"></button>
      </div>
      <div class="toast-body">${message}</div>
    </div>`;
  document.body.appendChild(toastContainer);
  setTimeout(() => toastContainer.remove(), 3200);
}
