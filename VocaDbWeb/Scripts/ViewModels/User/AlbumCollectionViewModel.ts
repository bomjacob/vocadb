﻿
namespace vdb.viewModels.user {
	
	import dc = vdb.dataContracts;
	import rep = vdb.repositories;

	export class AlbumCollectionViewModel {

		constructor(private userRepo: rep.UserRepository, private artistRepo: rep.ArtistRepository,
			private resourceRepo: rep.ResourceRepository,
			private languageSelection: string, private loggedUserId: number, private cultureCode: string,
			public publicCollection: boolean,
			initialize = true) {

			this.artistSearchParams = {
				acceptSelection: this.selectArtist
			};

			this.artistId.subscribe(this.updateResultsWithTotalCount);
			this.collectionStatus.subscribe(this.updateResultsWithTotalCount);
			this.paging.page.subscribe(this.updateResultsWithoutTotalCount);
			this.paging.pageSize.subscribe(this.updateResultsWithTotalCount);
			this.releaseEvent.id.subscribe(this.updateResultsWithTotalCount);
			this.searchTerm.subscribe(this.updateResultsWithTotalCount);
			this.sort.subscribe(this.updateResultsWithoutTotalCount);
			this.tag.subscribe(this.updateResultsWithTotalCount);

			if (initialize)
				this.init();

		}

		public artistId = ko.observable<number>(null);
		public artistName = ko.observable("");
		public artistSearchParams: vdb.knockoutExtensions.ArtistAutoCompleteParams;
		public collectionStatus = ko.observable('');
		public isInit = false;
		public loading = ko.observable(true); // Currently loading for data
		public page = ko.observableArray<dc.RatedSongForUserForApiContract>([]); // Current page of items
		public paging = new ServerSidePagingViewModel(20); // Paging view model
		public pauseNotifications = false;
		public releaseEvent = new BasicEntryLinkViewModel<dc.ReleaseEventContract>(null, null);
		public resources = ko.observable<dc.ResourcesContract>();
		public searchTerm = ko.observable("").extend({ rateLimit: { timeout: 300, method: "notifyWhenChangesStop" } });
		public sort = ko.observable("Name");
		public sortName = ko.computed(() => this.resources() != null ? this.resources().albumSortRuleNames[this.sort()] : "");
		public tag = ko.observable<dc.TagBaseContract>(null);
		public tagId = ko.computed(() => this.tag() ? this.tag().id : null);
		public tagName = ko.computed(() => this.tag() ? this.tag().name : null);
		public tagUrl = ko.computed(() => utils.EntryUrlMapper.details_tag_contract(this.tag()));
		public viewMode = ko.observable("Details");

		public releaseEventUrl = ko.computed(() => {
			return utils.EntryUrlMapper.details(models.EntryType.ReleaseEvent, this.releaseEvent.id());
		});

		public init = () => {

			if (this.isInit)
				return;

			this.resourceRepo.getList(this.cultureCode, ['albumCollectionStatusNames', 'albumMediaTypeNames', 'albumSortRuleNames', 'discTypeNames'], resources => {
				this.resources(resources);
				this.updateResultsWithTotalCount();
				this.isInit = true;
			});

		};


		public ratingStars = (userRating: number) => {

			var ratings = _.map([1, 2, 3, 4, 5], rating => { return { enabled: (Math.round(userRating) >= rating) } });
			return ratings;

		};

		public selectArtist = (selectedArtistId: number) => {
			this.artistId(selectedArtistId);
			this.artistRepo.getOne(selectedArtistId, artist => this.artistName(artist.name));
		};

		public updateResultsWithTotalCount = () => this.updateResults(true);
		public updateResultsWithoutTotalCount = () => this.updateResults(false);

		public updateResults = (clearResults: boolean = true) => {

			// Disable duplicate updates
			if (this.pauseNotifications)
				return;

			this.pauseNotifications = true;
			this.loading(true);

			if (clearResults)
				this.paging.page(1);

			var pagingProperties = this.paging.getPagingProperties(clearResults);

			this.userRepo.getAlbumCollectionList(this.loggedUserId, pagingProperties, this.languageSelection, this.searchTerm(),
				this.tagId(),
				this.artistId(),
				this.collectionStatus(), this.releaseEvent.id(), this.sort(),
				(result: any) => {

					this.pauseNotifications = false;

					if (pagingProperties.getTotalCount)
						this.paging.totalItems(result.totalCount);

					this.page(result.items);
					this.loading(false);

				});

		}

	}

}