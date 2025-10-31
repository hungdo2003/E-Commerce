package com.example.shoppeclone.ui.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;

import com.example.shoppeclone.R;
import com.example.shoppeclone.ui.CartActivity;
import com.google.android.material.bottomnavigation.BottomNavigationView;

import java.util.ArrayList;
import java.util.List;

public class HomeFragment extends Fragment {

    private ViewPager2 bannerViewPager;
    private Handler bannerHandler = new Handler();
    private int currentBannerPage = 0;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        // 🎨 Setup Banner ViewPager
        bannerViewPager = view.findViewById(R.id.bannerViewPager);
        setupBannerSlider();

        // 🔍 Search bar click
        View searchHint = view.findViewById(R.id.tvSearchHint);
        searchHint.setOnClickListener(v -> {
            // Chuyển sang tab Sản phẩm
            BottomNavigationView bottomNav = getActivity().findViewById(R.id.bottom_navigation);
            if (bottomNav != null) {
                bottomNav.setSelectedItemId(R.id.nav_products);
            }
        });

        // 🔥 Xem tất cả sản phẩm
        TextView tvViewAllProducts = view.findViewById(R.id.tvViewAllProducts);
        tvViewAllProducts.setOnClickListener(v -> {
            // Chuyển sang tab Sản phẩm
            BottomNavigationView bottomNav = getActivity().findViewById(R.id.bottom_navigation);
            if (bottomNav != null) {
                bottomNav.setSelectedItemId(R.id.nav_products);
            }
        });

        // 📞 Nút Liên hệ
        Button btnContactUs = view.findViewById(R.id.btnContactUs);
        btnContactUs.setOnClickListener(v -> {
            Toast.makeText(getContext(), "📞 Hotline: 1900 xxxx", Toast.LENGTH_LONG).show();
        });

        // 🛒 Nút Giỏ hàng
        Button btnViewCart = view.findViewById(R.id.btnViewCart);
        btnViewCart.setOnClickListener(v -> {
            Intent intent = new Intent(getActivity(), CartActivity.class);
            startActivity(intent);
        });

        return view;
    }

    private void setupBannerSlider() {
        // Danh sách màu banner demo (thay bằng hình ảnh thật nếu cần)
        List<Integer> bannerColors = new ArrayList<>();
        bannerColors.add(0xFFEF4444); // Red
        bannerColors.add(0xFF8B5CF6); // Purple
        bannerColors.add(0xFF3B82F6); // Blue
        bannerColors.add(0xFF10B981); // Green

        BannerAdapter adapter = new BannerAdapter(bannerColors);
        bannerViewPager.setAdapter(adapter);

        // Auto scroll banner
        Runnable bannerRunnable = new Runnable() {
            @Override
            public void run() {
                if (currentBannerPage == bannerColors.size()) {
                    currentBannerPage = 0;
                }
                bannerViewPager.setCurrentItem(currentBannerPage++, true);
                bannerHandler.postDelayed(this, 3000); // 3 giây đổi banner
            }
        };
        bannerHandler.postDelayed(bannerRunnable, 3000);
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        bannerHandler.removeCallbacksAndMessages(null);
    }

    // Adapter cho Banner
    private static class BannerAdapter extends RecyclerView.Adapter<BannerAdapter.BannerViewHolder> {
        private final List<Integer> colors;

        BannerAdapter(List<Integer> colors) {
            this.colors = colors;
        }

        @NonNull
        @Override
        public BannerViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view = LayoutInflater.from(parent.getContext())
                    .inflate(R.layout.item_banner, parent, false);
            return new BannerViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull BannerViewHolder holder, int position) {
            holder.imageView.setBackgroundColor(colors.get(position));
        }

        @Override
        public int getItemCount() {
            return colors.size();
        }

        static class BannerViewHolder extends RecyclerView.ViewHolder {
            ImageView imageView;

            BannerViewHolder(@NonNull View itemView) {
                super(itemView);
                imageView = itemView.findViewById(R.id.bannerImage);
            }
        }
    }
}
