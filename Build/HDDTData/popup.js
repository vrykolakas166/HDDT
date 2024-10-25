var startButton = document.getElementById("startBtn");
var log = document.getElementById("log");

window.addEventListener("load", () => {
  var isLoading = localStorage.getItem("IS_RUNNING");
  // localStorage.clear();
  if (isLoading === "true") {
    startButton.classList.remove("canhover");
    startButton.disabled = true;
    startButton.textContent = "Chờ xíu nha";
  }
});

startButton.addEventListener("click", async () => {
  startButton.classList.remove("canhover");
  startButton.disabled = true;
  startButton.textContent = "Chờ xíu nha";
  localStorage.setItem("IS_RUNNING", true);

  await executeScriptInActiveTab();
});

async function executeScriptInActiveTab() {
  try {
    // Wrap chrome.tabs.query in a Promise
    const tabs = await new Promise((resolve, reject) => {
      chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        if (chrome.runtime.lastError) {
          return reject(chrome.runtime.lastError);
        }
        resolve(tabs);
      });
    });

    // Check if any tabs were found
    if (tabs.length === 0) {
      throw new Error("No active tabs found.");
    }

    // Execute the script in the active tab
    await chrome.scripting.executeScript({
      target: { tabId: tabs[0].id },
      files: ["content.js"],
    });
  } catch (error) {
    console.error("Error executing script in active tab:", error);
  }
}

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.value && message.value === "finished") {
    // Finished
    localStorage.setItem("IS_RUNNING", false);
    log.textContent = "Xong rồi nè.";
    setTimeout(() => {
      log.textContent = "";
    }, 3000);

    startButton.classList.add("canhover");
    startButton.disabled = false;
    startButton.textContent = "Bắt đầu";
  } else if (message.value === "nodata") {
    // No data
    localStorage.setItem("IS_RUNNING", false);
    log.textContent = "Không có dữ liệu.";
    setTimeout(() => {
      log.textContent = "";
    }, 3000);

    startButton.classList.add("canhover");
    startButton.disabled = false;
    startButton.textContent = "Bắt đầu";
  } else {
    log.textContent = message.value;
  }
});
