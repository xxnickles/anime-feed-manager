const puppeteer = require('puppeteer');
const runTitles = async (storeFileFn) => {
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();
    
    await page.goto('https://subsplease.org/schedule/');
    await page.waitForSelector("table#full-schedule-table td.all-schedule-show");
    const data = await page.evaluate(() => {
        return Array.from(document.querySelectorAll('td.all-schedule-show a')).map(x => x.innerText);
    });
    await browser.close();

    storeFileFn('feed-titles.json', JSON.stringify(data));
};

module.exports.runTitles = runTitles;