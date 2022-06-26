const puppeteer = require('puppeteer');
const runAnidb = async (storeFileFn) => {
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();
    
    await page.goto('https://anidb.net/anime/season/?type.tvseries=1');
    // await page.goto('https://anidb.net/anime/season/?do=calendar&h=1&last.anime.month=13&last.anime.year=2021&type.tvseries=1');
    await page.waitForSelector("div.g_bubblewrap.g_bubble.container");
    const data = await page.evaluate(() => {

        const seasonInfomation = () => {
            const getDate = (str) => str.includes('/') ? str.split('/')[1] : str;
            const formatSeason = (str) => str === 'autumn' ? 'fall' : str;
            const titleParts = document.querySelector('div.g_section.content > h2 span').innerText.split(' ');
            return {
                season: formatSeason(titleParts[0].toLowerCase()),
                year: parseInt(getDate(titleParts[1]))
            }
        }

        const imgObjList = [].slice.call(document.querySelectorAll('div.g_bubble.box'))
            .map(card => {

                const getImage = () => {
                    const cleanSrc = (src) => src.replace('.jpg-thumb','')
                    const image = card.querySelector('div.thumb.image img');
                    return cleanSrc(image.src);
                }

                return {
                    title: card.querySelector('div.wrap.name a').innerText,
                    url: getImage()                 
                }
            }
            );

        return {
            seasonInfo: seasonInfomation(),
            imagesInfo: imgObjList
        }
    });
    await browser.close();

    storeFileFn(`images-${data.seasonInfo.season}-${data.seasonInfo.year}.json`, JSON.stringify(data));
};

module.exports.runAnidb = runAnidb;