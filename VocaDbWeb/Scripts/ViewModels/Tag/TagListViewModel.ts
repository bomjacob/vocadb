﻿
module vdb.viewModels.tags {
	
	import dc = dataContracts;

	export class TagListViewModel {
		
		private static maxDisplayedTags = 4;

		constructor(tagUsages: dc.tags.TagUsageForApiContract[]) {
			
			this.tagUsages = ko.observableArray(tagUsages);

			if (tagUsages.length <= TagListViewModel.maxDisplayedTags + 1)
				this.expanded(true);

			this.displayedTagUsages = ko.computed(() =>
				(this.expanded() ? this.tagUsages() : _.take(this.tagUsages(), TagListViewModel.maxDisplayedTags))
			);

		}

		public displayedTagUsages: KnockoutComputed<dc.tags.TagUsageForApiContract[]>;

		public getTagUrl = (tag: dc.tags.TagUsageForApiContract) => {
			return utils.EntryUrlMapper.details_tag(tag.tag.id, tag.tag.urlSlug);
		}

		public expanded = ko.observable(false);

		public tagUsages: KnockoutObservableArray<dc.tags.TagUsageForApiContract>;

	}

} 