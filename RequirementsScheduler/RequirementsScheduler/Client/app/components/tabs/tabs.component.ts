import { Component, ContentChildren, QueryList, OnInit } from '@angular/core';
import { TabComponent } from '../tab/tab.component';

@Component({
    selector: 'tabs',
    templateUrl: './tabs.component.html'
})
export class TabsComponent implements OnInit {
    @ContentChildren(TabComponent) tabs: QueryList<TabComponent>;

    constructor() {
        this.tabs = new QueryList<TabComponent>();
    }

    ngOnInit() {
        const activeTabs = this.tabs.filter((tab) => tab.active);

        if (activeTabs.length === 0) {
            this.selectTab(this.tabs.first);
        }
    }

    selectTab(tab: TabComponent) {

        this.tabs.toArray().forEach((tab) => tab.activate(false));
        if (tab) {
            tab.activate(true);
        } else if (this.tabs.first) {
            this.tabs.first.activate(true);
        }
    }
}
