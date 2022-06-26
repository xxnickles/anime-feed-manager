const puppeteer = require('puppeteer');

const runLiveChart = async (storeFileFn) => {
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();
    await page.goto('https://www.livechart.me');
    await page.waitForTimeout(1000);
    const data = await page.evaluate(() => {

        const seasonInfomation = () => {

            const titleParts = document.querySelector('h1').innerText.split(' ');
            return {
                season: titleParts[0].toLowerCase(),
                year: parseInt(titleParts[1])
            }
        }

        const imgObjList = [].slice.call(document.querySelectorAll('div.anime-card'))
            .map(card => {
                const checkIfLeftover = () => {
                    const extras = [].slice.call(card.querySelectorAll('div.anime-date'));
                    if (!extras) return false;
                    return extras.reduce((acc, curr) => acc || curr.innerText === 'Ongoing', false);
                };

                const getImage = () => {
                    const image = card.querySelector('div.poster-container > img');
                    const imgSrc = image.classList.contains('lazyload') ? image.getAttribute('data-src') : image.src;
                    return imgSrc.replace('small', 'large')
                }

                return {
                    title: card.querySelector('h3 > a').innerText,
                    image: getImage(),
                    leftover: checkIfLeftover()
                }
            }
            )
            .filter(info => !info.leftover)
            .map(x => ({
                title: x.title,
                url: x.image
            }));

        return {
            seasonInfo: seasonInfomation(),
            imagesInfo: imgObjList
        }
    });
    await browser.close();    
    storeFileFn(`images-${data.seasonInfo.season}-${data.seasonInfo.year}.json`, JSON.stringify(data));
};


module.exports.runLiveChart = runLiveChart;