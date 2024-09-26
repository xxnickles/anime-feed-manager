﻿function gridFilter() {
    return {
        filters: [],
        toggleFilter(filter) {
            if (this.filters.includes(filter)) {
                this.filters = this.filters.filter(f => f !== filter);
            } else {
                this.filters = [...this.filters, filter];
            }
            this.applyFilters();
        },
        applyFilters() {
            let items = document.querySelectorAll('#main-grid > article');
            let showAllItems = this.filters.length === 0;
            items.forEach(item => {
                let showItem = showAllItems;
                if (!showItem) {
                    showItem = this.filters.every(filter => {
                        return item.dataset[filter] === 'true'});
                }
                item.style.display = showItem ? '' : 'none';
            });
        }
    }
}