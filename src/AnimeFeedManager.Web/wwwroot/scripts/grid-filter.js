function getItemFilterState(filters) {
    let items = Array.from(document.querySelectorAll('div#main-grid > section > article footer span.filters'));
    let showAllItems = filters.length === 0;
    return items.map(item => {
        let showItem = showAllItems;
        if (!showItem) {
            showItem = filters.every(filter => item.dataset[filter] === 'true');
        }
        return {
            item, showItem
        };
    });
}

function gridFilter() {
    return {
        filters: [],
        motionOk: window.matchMedia('(prefers-reduced-motion: no-preference)').matches,
        toggleFilter(filter) {
            if (this.filters.includes(filter)) {
                this.filters = this.filters.filter(f => f !== filter);
            } else {
                this.filters = [...this.filters, filter];
            }

            const state = getItemFilterState(this.filters);
            const itemsToShow = state.filter(s => s.showItem).length;
            document.startViewTransition && this.motionOk && (itemsToShow < 20) ? document.startViewTransition(() => this.applyFilters(state)) : this.applyFilters(state);
        },
        applyFilters(itemsState) {
            itemsState.forEach(state => {
                state.item.closest('article').style.display = state.showItem ? '' : 'none';
            });
        }
    }
}