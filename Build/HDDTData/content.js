// content.js

var tabSell = "Tra cứu hóa đơn điện tử bán ra";
var tabBuy = "Tra cứu hóa đơn điện tử mua vào";
var activeTab;

var currentIndex = 0;
var currentPage = 1;
var lastPage = 0;

function exportTableToJSON(filename) {
  const table = document.querySelector(".res-tb");
  const rows = table.querySelectorAll("tbody tr");
  const jsonData = [];

  // Header row
  const headers = Array.from(table.querySelectorAll("thead th")).map((th) =>
    th.textContent.trim()
  );

  // Data rows
  rows.forEach((row) => {
    const cells = row.querySelectorAll("td");
    const rowData = {};
    headers.forEach((header, index) => {
      rowData[header] = cells[index] ? cells[index].textContent.trim() : "";
    });
    jsonData.push(rowData);
  });

  // Create a Blob from the JSON data
  const blob = new Blob([JSON.stringify(jsonData, null, 2)], {
    type: "application/json;charset=utf-8;",
  });

  // Create a link to download the Blob
  const link = document.createElement("a");
  const url = URL.createObjectURL(blob);
  link.setAttribute("href", url);
  link.setAttribute("download", filename);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

function waitForElement(selector, callback) {
  const interval = setInterval(() => {
    const element = document.querySelector(selector);
    if (element) {
      clearInterval(interval);
      callback(element); // Call the callback function with the found element
    }
  }, 100); // Check every 100 milliseconds
}

function checkActiveTab() {
  var tabs = document.getElementsByClassName("ant-tabs-tab-active");
  for (let i = 0; i < tabs.length; i++) {
    var e = tabs[i];
    if (e.textContent.includes(tabSell) || e.textContent.includes(tabBuy)) {
      activeTab = e;
      return;
    }
  }
}

function isFirstTab() {
  if (activeTab) {
    if (activeTab.textContent.includes(tabSell)) {
      return true;
    }
  }

  return false;
}

function getPage() {
  var pages = document.getElementsByClassName(
    "styles__PageIndex-sc-eevgvg-3 fWbeIm"
  );
  if (isFirstTab()) {
    return pages[0];
  } else {
    return pages[1];
  }
}

function getCurrentPages() {
  var tempPage = getPage().textContent.trim().split("/");
  currentPage = parseInt(tempPage[0].trim());
  lastPage = parseInt(tempPage[1].trim());
}

function getNextPageButton() {
  var npbs = document.getElementsByClassName(
    "ant-btn ButtonAnt__Button-sc-p5q16s-0 kNMAep ant-btn-primary ant-btn-icon-only"
  );
  if (isFirstTab()) {
    return npbs[1];
  } else {
    return npbs[3];
  }
}

// Function to start clicking rows
function startClicking() {
  // check page
  checkActiveTab();
  getCurrentPages();

  function clickNextPage() {
    if (currentPage < lastPage) {
      // get next button
      const icons = document.querySelectorAll(".anticon.anticon-right");
      let icon;
      if (isFirstTab()) {
        icon = icons[1];
      } else {
        icon = icons[3];
      }
      if (icon) {
        const buttonNext = icon.closest("button");
        buttonNext.click();
        notify(`Sang trang ${currentPage + 1}...`);

        // start again if new page, after 3 seconds for loading new
        setTimeout(() => {
          currentPage += 1;
          currentIndex = 0;
          clickRow();
        }, 3000);
      }
    } else {
      notify("finished");
      console.log("Done.");
    }
  }

  function getDefaultValue(value) {
    return value === null || value.trim() === "" ? null : value;
  }

  // Function to click each row
  function clickRow() {
    const rows = document.querySelectorAll(".ant-table-tbody tr");
    const rowCount = rows.length;

    if (rowCount === 0) {
      console.log("No rows found.");
      notify("nodata");
      return;
    }

    if (currentIndex < rowCount) {
      notify(`[Trang ${currentPage}] STT: ${currentIndex + 1}`);
      const row = rows[currentIndex];

      // Find the cell containing "Số hóa đơn"
      const invoiceSymbolCell = row.querySelector("td:nth-child(3)");
      const invoiceNumberCell = row.querySelector("td:nth-child(4)");
      const invoiceDateCell = row.querySelector("td:nth-child(5)");
      const mstCell = row.querySelector("td:nth-child(6)");

      const invoiceSymbol = getDefaultValue(invoiceSymbolCell.textContent);
      const invoiceNumber = getDefaultValue(invoiceNumberCell.textContent);
      const invoiceDate = getDefaultValue(
        invoiceDateCell.textContent.replaceAll("/", "")
      );
      const mst = getDefaultValue(
        mstCell.textContent
          .split("Tên người mua")[0]
          .replace("MST người mua: ", "")
          .trim()
      );

      let fileName = `data_${invoiceSymbol}_${invoiceNumber}_${invoiceDate}_${mst}`;

      row.click(); // Simulate click on the row
      console.log(`Clicked row ${currentIndex + 1}`);

      // Wait for 1 second before clicking
      setTimeout(() => {
        const clipPath = document.querySelector(
          "clipPath#clip-icon_xemchitiet"
        );

        if (clipPath) {
          const button = clipPath.closest("button");
          button.click();
        }
      }, 1000); // Delay of 1000 milliseconds

      waitForElement(".ant-modal-close-x", (closeButton) => {
        setTimeout(() => {
          // You can proceed with your actions here
          exportTableToJSON(`${fileName}.json`);
          closeButton.click(); // Click the close button

          // Now call clickRow for the next index
          clickRow();
        }, 1000); // Delay before clicking the close button
      });

      currentIndex++;
    } else {
      console.log("Ready to next page");
      // click if there is next page
      clickNextPage();
    }
  }

  clickRow();
}

function notify(val) {
  chrome.runtime.sendMessage({ value: val });
}

window.addEventListener("beforeunload", (event) => {
  notify("finished");
});

// Start clicking when the script loads
startClicking();
