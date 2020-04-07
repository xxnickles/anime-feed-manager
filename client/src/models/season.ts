 enum Season {
  spring = 'spring',
  summer = 'summer',
  fall = 'fall',
  winter = 'winter',
}

interface SeasonInformation {
  season: Season,
  year: number
}

export {
  Season,
  SeasonInformation
}
