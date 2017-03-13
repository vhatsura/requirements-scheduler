import { Component, ContentChildren, QueryList, OnInit } from "@angular/core";
import { TabComponent } from "../tab/tab.component"

@Component({
    selector: "tabs",
    template: require("./tabs.component.html")
})
export class TabsComponent implements OnInit {
    @ContentChildren(TabComponent) tabs: QueryList<TabComponent>;

    ngOnInit() {
        let activeTabs = this.tabs.filter((tab) => tab.active);

        if (activeTabs.length === 0) {
            this.selectTab(this.tabs.first);
        }
    }

    selectTab(tab: TabComponent) {
        this.tabs.toArray().forEach(tab => tab.activate(false));

        tab.activate(true);
    }
}
