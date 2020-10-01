import { StoreConfig, Store } from "@datorama/akita";

interface UserState {
  subscriptions: string[];
  isLogged: boolean;
  currentUser: string;
  interested: string[];
}

function emptyUserState() : UserState {
  return {
    currentUser: '',
    isLogged: false,
    subscriptions: [],
    interested: []
  }
}

@StoreConfig({ name: 'animeList' })
class UserStore extends Store<UserState> {

  constructor() {
    super(emptyUserState());
  }
}

const userStore = new UserStore();

export {
  UserState,
  UserStore,
  userStore,
  emptyUserState
}
