﻿module Scrappers.Scrapping

open PuppeteerSharp
open Scrappers.Common.Types

let SubsPleaseTitleScrapper (browser: Browser) subsPlease =
    async {
        use! page = browser.NewPageAsync() |> Async.AwaitTask
        let! _ = page.GoToAsync(subsPlease) |> Async.AwaitTask

        let! _ =
            page.WaitForSelectorAsync("table#full-schedule-table td.all-schedule-show")
            |> Async.AwaitTask

        let jsSelection = @"() => {
                        return Array.from(document.querySelectorAll('td.all-schedule-show a')).map(x => x.innerText);
                         }"

        let! titles =
            page.EvaluateFunctionAsync<string []>(jsSelection)
            |> Async.AwaitTask

        let! _ = browser.CloseAsync() |> Async.AwaitTask

        return { titles = titles }
    }

let EraiTitleScrapper (browser: Browser) eraiUrl =
    async {
        use! page = browser.NewPageAsync() |> Async.AwaitTask
        let! _ = page.GoToAsync(eraiUrl) |> Async.AwaitTask

        let! _ =
            page.WaitForSelectorAsync("body.multiple-domain-spa-erai-raws-info")
            |> Async.AwaitTask

        let jsSelection = @"() => {
                        return Array.from(document.querySelectorAll('h6.button.button5.hhhh5 a')).map(x => x.innerText);
                         }"

        let! titles =
            page.EvaluateFunctionAsync<string []>(jsSelection)
            |> Async.AwaitTask

        let! _ = browser.CloseAsync() |> Async.AwaitTask

        return { titles = titles }
    }

let AniDbImageScrapper (browser: Browser) url =
    async {
        use! page = browser.NewPageAsync() |> Async.AwaitTask
        let! _ = page.GoToAsync(url) |> Async.AwaitTask

        let! _ =
            page.WaitForSelectorAsync("div.g_bubblewrap.g_bubble.container")
            |> Async.AwaitTask

        let jsSelection = @"() => {

            const seasonInformation = () => {
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
            seasonInfo: seasonInformation(),
            imagesInfo: imgObjList
        }
    }"

        let! result =
            page.EvaluateFunctionAsync<ImageProcessInfo>(jsSelection)
            |> Async.AwaitTask

        let! _ = browser.CloseAsync() |> Async.AwaitTask
        return result
    }

let LiveChartImageScrapper (browser: Browser) url =
    async {
        use! page = browser.NewPageAsync() |> Async.AwaitTask
        let! _ = page.GoToAsync(url) |> Async.AwaitTask
        let! _ = page.WaitForTimeoutAsync(1000) |> Async.AwaitTask

        let jsSelection = @"() => {
                       const seasonInformation = () => {
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
                            seasonInfo: seasonInformation(),
                            imagesInfo: imgObjList
                        }
                       }"

        let! result =
            page.EvaluateFunctionAsync<ImageProcessInfo>(jsSelection)
            |> Async.AwaitTask

        let! _ = browser.CloseAsync() |> Async.AwaitTask
        return result
    }
