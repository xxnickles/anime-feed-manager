import { StoreConfig, Store } from "@datorama/akita";

interface UserState {
  subscriptions: string[];
  isLogged: boolean;
  currentUser: string;
}

function initailState() : UserState {
  return {
    currentUser: '',
    isLogged: false,
    subscriptions: []
  }
}

@StoreConfig({ name: 'animeList' })
class UserStore extends Store<UserState> {

  constructor() {
    super(initailState());
  }
}

const userStore = new UserStore();

export {
  UserState,
  UserStore,
  userStore
}
